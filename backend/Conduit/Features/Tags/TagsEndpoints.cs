using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.RequestHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Features.Tags;

public static class TagsEndpoints
{
    public static IEndpointRouteBuilder MapTagsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var tags = endpoints.MapGroup("/tags").WithTags("Tags");

        tags.MapGet("", ListTagsAsync);

        return endpoints;
    }

    private static Task<TagsEnvelope> ListTagsAsync(IQueryHandler<List.Query, TagsEnvelope> queryHandler, CancellationToken cancellationToken) =>
        queryHandler.Handle(new List.Query(), cancellationToken);
}
