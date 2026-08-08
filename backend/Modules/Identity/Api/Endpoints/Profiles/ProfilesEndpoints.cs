using Conduit.Shared.Infrastructure.ApiEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Identity.Api.Endpoints.Profiles;

public static class ProfilesEndpoints
{
    public static IEndpointRouteBuilder MapProfilesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var profiles = endpoints.MapGroup("/profiles")
            .WithTags("Profile");

        profiles.AddEndpoint<GetProfileEndpoint>();
        profiles.AddEndpoint<FollowUserEndpoint>();
        profiles.AddEndpoint<UnfollowUserEndpoint>();

        return endpoints;
    }
}
