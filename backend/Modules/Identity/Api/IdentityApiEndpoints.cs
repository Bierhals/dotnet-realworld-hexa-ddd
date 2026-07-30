using Conduit.Identity.Api.Endpoints.Profiles;
using Conduit.Identity.Api.Endpoints.Users;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Identity.Api;

public static class IdentityApiEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapUsersEndpoints();
        endpoints.MapProfilesEndpoints();

        return endpoints;
    }
}
