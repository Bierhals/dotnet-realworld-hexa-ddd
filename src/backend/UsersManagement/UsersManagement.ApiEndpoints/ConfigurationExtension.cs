using Conduit.UsersManagement.ApiEndpoints.Users;
using Microsoft.AspNetCore.Builder;

namespace Conduit.UsersManagement.ApiEndpoints;

public static class ConfigurationExtension
{
    public static void RegisterUserManagementEndpoints(this WebApplication app)
    {
        app.MapGet("/api/user", GetCurrentUserEndpoint.HandleAsync);
    }
}
