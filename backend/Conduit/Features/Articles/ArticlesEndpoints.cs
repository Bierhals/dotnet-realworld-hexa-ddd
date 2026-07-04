using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure.Security;
using Conduit.Shared.RequestHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Features.Articles;

public static class ArticlesEndpoints
{
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
        IQueryHandler<List.Query, ArticlesEnvelope> queryHandler,
        string? tag,
        string? author,
        string? favorited,
        int? limit,
        int? offset,
        CancellationToken cancellationToken)
    {
        return queryHandler.Handle(
            new List.Query(
                tag ?? string.Empty,
                author ?? string.Empty,
                favorited ?? string.Empty,
                limit,
                offset
            ),
            cancellationToken
        );
    }

    private static Task<ArticlesEnvelope> GetFeedArticlesAsync(
        IQueryHandler<List.Query, ArticlesEnvelope> queryHandler,
        string? tag,
        string? author,
        string? favorited,
        int? limit,
        int? offset,
        CancellationToken cancellationToken)
    {
        return queryHandler.Handle(
            new List.Query(
                tag ?? string.Empty,
                author ?? string.Empty,
                favorited ?? string.Empty,
                limit,
                offset
            )
            {
                IsFeed = true,
            },
            cancellationToken
        );
    }

    private static Task<ArticleEnvelope> GetArticleAsync(IQueryHandler<Details.Query, ArticleEnvelope> queryHandler, [Required] string slug, CancellationToken cancellationToken)
    {
        return queryHandler.Handle(new Details.Query(slug), cancellationToken);
    }

    private static Task<ArticleEnvelope> PostArticleAsync(ICommandHandler<Create.Command, ArticleEnvelope> commandHandler, Create.Command command, CancellationToken cancellationToken)
    {
        return commandHandler.Handle(command, cancellationToken);
    }

    private static Task<ArticleEnvelope> PutArticleAsync(
        ICommandHandler<Edit.Command, ArticleEnvelope> commandHandler,
        [Required] string slug,
        Edit.Model model,
        CancellationToken cancellationToken)
    {
        return commandHandler.Handle(new Edit.Command(model, slug), cancellationToken);
    }

    private static Task DeleteArticleAsync(ICommandHandler<Delete.Command> commandHandler, [Required] string slug, CancellationToken cancellationToken)
    {
        return commandHandler.Handle(new Delete.Command(slug), cancellationToken);
    }
}
