using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using List = Conduit.Features.Tags.List;

namespace Conduit.Features.Tags;

public static class TagsEndpoints
{
    public static IEndpointRouteBuilder MapTagsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var tags = endpoints.MapGroup("/tags").WithTags("Tags");

        tags.MapGet("", ListTagsAsync);

        return endpoints;
    }

    private static Task<TagsEnvelope> ListTagsAsync(IMediator mediator, CancellationToken cancellationToken) =>
        mediator.Send(new List.Query(), cancellationToken);
}
