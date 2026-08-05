using Conduit.Tags.Core.Api.Endpoints.Tags;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Tags.Core.Api;

public static class TagsApiEndpoints
{
    public static IEndpointRouteBuilder MapTagsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapTagsGroup();

        return endpoints;
    }
}
