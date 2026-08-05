using Conduit.Shared.Infrastructure.ApiEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Tags.Core.Api.Endpoints.Tags;

public static class TagsEndpoints
{
    public static IEndpointRouteBuilder MapTagsGroup(this IEndpointRouteBuilder endpoints)
    {
        var tags = endpoints.MapGroup("/tags")
            .WithTags("Tags");

        tags.AddEndpoint<ListTagsEndpoint>();

        return endpoints;
    }
}
