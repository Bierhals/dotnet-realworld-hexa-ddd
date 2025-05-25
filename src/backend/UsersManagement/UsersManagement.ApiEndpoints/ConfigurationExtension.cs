using Conduit.UsersManagement.ApiEndpoints.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Conduit.Shared.ApiEndpoints;

namespace Conduit.UsersManagement.ApiEndpoints;

public static class ConfigurationExtension
{
    public static void MapUserManagementEndpoints(this IEndpointRouteBuilder app)
    {
        app.AddEndpoint<GetCurrentUserEndpoint>();
    }
}
