using Conduit.Shared.Infrastructure.ApiEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Identity.Api.Endpoints.Users;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var users = endpoints.MapGroup("/users")
            .WithTags("User and Authentication");
        users.AddEndpoint<RegisterUserEndpoint>();
        users.AddEndpoint<LoginUserEndpoint>();

        var currentUser = endpoints.MapGroup("/user")
            .WithTags("User and Authentication")
            .RequireAuthorization();
        currentUser.AddEndpoint<GetCurrentUserEndpoint>();
        currentUser.AddEndpoint<UpdateCurrentUserEndpoint>();

        return endpoints;
    }
}
