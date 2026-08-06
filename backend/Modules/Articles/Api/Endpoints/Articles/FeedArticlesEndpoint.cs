using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Queries.ArticleFeed;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Conduit.Shared.Infrastructure.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Articles.Api.Endpoints.Articles;

internal sealed class FeedArticlesEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("feed", HandleAsync)
            .RequireAuthorization()
            .WithSummary("Get recent articles from users you follow")
            .WithDescription("Get most recent articles from users you follow. Use query parameters to limit. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#feed-articles\">Conduit Spec for feed articles endpoint</a>")
            .Produces<ArticlesEnvelope>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Results<Ok<ArticlesEnvelope>, ProblemHttpResult>> HandleAsync(
        [Description("The number of items to skip before starting to collect the result set.")] int? offset,
        [Description("The numbers of items to return.")][DefaultValue(20)] int? limit,
        ICqrsMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new ArticleFeedQuery
        {
            Limit = limit ?? 20,
            Offset = offset ?? 0,
        };

        var articles = await mediator.Send(query, cancellationToken);

        return articles.IsError
            ? articles.Errors.ToProblemResult()
            : TypedResults.Ok(ArticleEnvelopeFactory.Create(articles.Value));
    }
}
