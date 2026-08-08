using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Api.Endpoints.Articles.Dtos;
using Conduit.Articles.Application.Queries.ArticleList;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Conduit.Shared.Infrastructure.ErrorHandling;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Articles.Api.Endpoints.Articles;

internal sealed class ListArticlesEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", HandleAsync)
            .AllowAnonymous()
            .WithSummary("Get recent articles globally")
            .WithDescription("Get most recent articles globally. Use query parameters to filter results. Auth is optional<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#list-articles\">Conduit Spec for list articles endpoint</a>")
            .Produces<ArticlesEnvelope>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static Task<Results<Ok<ArticlesEnvelope>, ProblemHttpResult>> HandleAsync(
        [Description("Filter by tag")] string? tag,
        [Description("Filter by author (username)")] string? author,
        [Description("Filter by favorites of a user (username)")] string? favorited,
        [Description("The number of items to skip before starting to collect the result set.")] int? offset,
        [Description("The numbers of items to return.")][DefaultValue(20)] int? limit,
        ICqrsMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new ArticleListQuery
        {
            Tag = tag,
            Author = author,
            FavoritedBy = favorited,
            Limit = limit ?? 20,
            Offset = offset ?? 0,
        };

        return mediator.Send(query, cancellationToken)
            .Then(ArticleEnvelopeFactory.Create)
            .ToOkResult();
    }
}
