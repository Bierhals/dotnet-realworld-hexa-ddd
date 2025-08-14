using Conduit.Shared.ApiEndpoints;
using Conduit.UsersManagement.ApiEndpoints.CreateUser;
using Conduit.UsersManagement.ApiEndpoints.Users;
using Microsoft.AspNetCore.Routing;

namespace Conduit.UsersManagement.ApiEndpoints;

public static class ConfigurationExtension
{
    public static void MapUserManagementEndpoints(this IEndpointRouteBuilder app)
    {
        app.AddEndpoint<GetCurrentUserEndpoint>();
        app.AddEndpoint<CreateUserEndpoint>();
    }
}
