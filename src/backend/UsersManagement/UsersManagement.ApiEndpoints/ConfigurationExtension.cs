using Conduit.Shared.ApiEndpoints;
using Conduit.UsersManagement.ApiEndpoints.CreateUser;
using Conduit.UsersManagement.ApiEndpoints.FollowUserByUsername;
using Conduit.UsersManagement.ApiEndpoints.GetProfileByUsername;
using Conduit.UsersManagement.ApiEndpoints.Login;
using Conduit.UsersManagement.ApiEndpoints.UnfollowUserByUsername;
using Conduit.UsersManagement.ApiEndpoints.UpdateCurrentUser;
using Conduit.UsersManagement.ApiEndpoints.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.UsersManagement.ApiEndpoints;

public static class ConfigurationExtension
{
    public static void MapUserManagementEndpoints(this IEndpointRouteBuilder app)
    {
        app.AddEndpoint<LoginEndpoint>();
        app.MapUserEndpoints();
        app.MapProfileEndpoints();
    }

    private static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user")
            .WithTags("User and Authentication");

        group.AddEndpoint<GetCurrentUserEndpoint>();
        group.AddEndpoint<CreateUserEndpoint>();
        group.AddEndpoint<UpdateCurrentUserEndpoint>();
    }

    private static void MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/profiles")
            .WithTags("Profile");

        group.AddEndpoint<GetProfileByUsernameEndpoint>();
        group.AddEndpoint<FollowUserByUsernameEndpoint>();
        group.AddEndpoint<UnfollowUserByUsernameEndpoint>();
    }

    public static void ConfigureUserManagementJsonOptions(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, UserManagementSerializerContext.Default);
        });
    }
}
