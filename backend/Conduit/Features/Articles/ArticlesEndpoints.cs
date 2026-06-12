using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure.Security;
using MediatR;
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
        IMediator mediator,
        string? tag,
        string? author,
        string? favorited,
        int? limit,
        int? offset,
        CancellationToken cancellationToken)
    {
        return mediator.Send(
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
        IMediator mediator,
        string? tag,
        string? author,
        string? favorited,
        int? limit,
        int? offset,
        CancellationToken cancellationToken)
    {
        return mediator.Send(
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

    private static Task<ArticleEnvelope> GetArticleAsync(IMediator mediator, [Required] string slug, CancellationToken cancellationToken)
    {
        return mediator.Send(new Details.Query(slug), cancellationToken);
    }

    private static Task<ArticleEnvelope> PostArticleAsync(IMediator mediator, Create.Command command, CancellationToken cancellationToken)
    {
        return mediator.Send(command, cancellationToken);
    }

    private static Task<ArticleEnvelope> PutArticleAsync(
        IMediator mediator,
        [Required] string slug,
        Edit.Model model,
        CancellationToken cancellationToken)
    {
        return mediator.Send(new Edit.Command(model, slug), cancellationToken);
    }

    private static Task DeleteArticleAsync(IMediator mediator, [Required] string slug, CancellationToken cancellationToken)
    {
        return mediator.Send(new Delete.Command(slug), cancellationToken);
    }
}
