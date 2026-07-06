using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure.Security;
using Conduit.Shared.RequestHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Features.Articles;

public static class ArticlesEndpoints
{
    private const string GetArticleRouteName = "GetArticle";

    public static IEndpointRouteBuilder MapArticlesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var articles = endpoints.MapGroup("/articles")
            .WithTags("Articles")
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtIssuerOptions.Schemes });

        articles.MapGet("", GetArticlesAsync)
            .AllowAnonymous()
            .WithSummary("Get recent articles globally")
            .WithDescription("Get most recent articles globally. Use query parameters to filter results. Auth is optional<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#list-articles\">Conduit Spec for list articles endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
        articles.MapGet("feed", GetFeedArticlesAsync)
            .WithSummary("Get recent articles from users you follow")
            .WithDescription("Get most recent articles from users you follow. Use query parameters to limit. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#feed-articles\">Conduit Spec for feed articles endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
        articles.MapGet("{slug}", GetArticleAsync)
            .AllowAnonymous()
            .WithName(GetArticleRouteName)
            .WithSummary("Get an article")
            .WithDescription("Get an article. Auth not required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#get-article\">Conduit Spec for get article endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
        articles.MapPost("", PostArticleAsync)
            .WithSummary("Create an article")
            .WithDescription("Create an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#create-article\">Conduit Spec for create article endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
        articles.MapPut("{slug}", PutArticleAsync)
            .WithSummary("Update an article")
            .WithDescription("Update an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#update-article\">Conduit Spec for update article endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status403Forbidden)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
        articles.MapDelete("{slug}", DeleteArticleAsync)
            .WithSummary("Delete an article")
            .WithDescription("Delete an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#delete-article\">Conduit Spec for delete article endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status403Forbidden)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }

    private static Task<ArticlesEnvelope> GetArticlesAsync(
        [Description("Filter by tag")]
        string? tag,
        [Description("Filter by author (username)")]
        string? author,
        [Description("Filter by favorites of a user (username)")]
        string? favorited,
        [Description("The number of items to skip before starting to collect the result set.")]
        int? offset,
        [Description("The numbers of items to return.")]
        [DefaultValue(20)]
        int? limit,
        IQueryHandler<List.Query, ArticlesEnvelope> queryHandler,
        CancellationToken cancellationToken)
    {
        return queryHandler.Handle(
            new List.Query(
                tag ?? string.Empty,
                author ?? string.Empty,
                favorited ?? string.Empty,
                limit ?? 20,
                offset
            ),
            cancellationToken
        );
    }

    private static Task<ArticlesEnvelope> GetFeedArticlesAsync(
        [Description("The number of items to skip before starting to collect the result set.")]
        int? offset,
        [Description("The numbers of items to return.")]
        [DefaultValue(20)]
        int? limit,
        IQueryHandler<List.Query, ArticlesEnvelope> queryHandler,
        CancellationToken cancellationToken)
    {
        return queryHandler.Handle(
            new List.Query(
                string.Empty,
                string.Empty,
                string.Empty,
                limit ?? 20,
                offset
            )
            {
                IsFeed = true,
            },
            cancellationToken
        );
    }

    private static Task<ArticleEnvelope> GetArticleAsync(
        [Required]
        [Description("Slug of the article to get")]
        string slug,
        IQueryHandler<Details.Query, ArticleEnvelope> queryHandler,
        CancellationToken cancellationToken)
    {
        return queryHandler.Handle(new Details.Query(slug), cancellationToken);
    }

    private static async Task<Created<ArticleEnvelope>> PostArticleAsync(
        [Description("The article to create")]
        Create.Command command,
        ICommandHandler<Create.Command, ArticleEnvelope> commandHandler,
        LinkGenerator linkGenerator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var envelope = await commandHandler.Handle(command, cancellationToken);
        var location = linkGenerator.GetUriByName(httpContext, GetArticleRouteName, new { slug = envelope.Article.Slug });
        return TypedResults.Created(location, envelope);
    }

    private static Task<ArticleEnvelope> PutArticleAsync(
        [Required]
        [Description("The slug of the article to update")]
        string slug,
        [Description("The article to update")]
        Edit.Model model,
        ICommandHandler<Edit.Command, ArticleEnvelope> commandHandler,
        CancellationToken cancellationToken)
    {
        return commandHandler.Handle(new Edit.Command(model, slug), cancellationToken);
    }

    private static async Task<NoContent> DeleteArticleAsync(
        [Required]
        [Description("The slug of the article to delete")]
        string slug,
        ICommandHandler<Delete.Command> commandHandler,
        CancellationToken cancellationToken)
    {
        await commandHandler.Handle(new Delete.Command(slug), cancellationToken);
        return TypedResults.NoContent();
    }
}
