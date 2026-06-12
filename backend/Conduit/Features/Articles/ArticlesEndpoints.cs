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

        articles.MapGet("", GetArticlesAsync).AllowAnonymous();
        articles.MapGet("feed", GetFeedArticlesAsync).AllowAnonymous();
        articles.MapGet("{slug}", GetArticleAsync).AllowAnonymous();
        articles.MapPost("", PostArticleAsync);
        articles.MapPut("{slug}", PutArticleAsync);
        articles.MapDelete("{slug}", DeleteArticleAsync);

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
