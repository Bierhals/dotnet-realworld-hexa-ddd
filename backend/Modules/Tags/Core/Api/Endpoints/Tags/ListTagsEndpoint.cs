using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Conduit.Shared.Infrastructure.ErrorHandling;
using Conduit.Tags.Core.Application.Queries.TagCatalog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Tags.Core.Api.Endpoints.Tags;

internal sealed class ListTagsEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", HandleAsync)
            .WithSummary("Get tags")
            .WithDescription("Get tags. Auth not required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#tags\">Conduit Spec for tags endpoint</a>")
            .Produces<TagsEnvelope>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Results<Ok<TagsEnvelope>, ProblemHttpResult>> HandleAsync(
        ICqrsMediator mediator,
        CancellationToken cancellationToken)
    {
        var tags = await mediator.Send(new TagCatalogQuery(), cancellationToken);

        return tags.IsError
            ? tags.Errors.ToProblemResult()
            : TypedResults.Ok(new TagsEnvelope(tags.Value));
    }
}
