using Conduit.Identity.Application.Commands.AuthenticateUser;
using Conduit.Identity.Application.Commands.FollowUser;
using Conduit.Identity.Application.Commands.RegisterUser;
using Conduit.Identity.Application.Commands.UnfollowUser;
using Conduit.Identity.Application.Commands.UpdateUser;
using Conduit.Identity.Application.Queries.CurrentUser;
using Conduit.Identity.Application.Queries.Profile;
using Conduit.Identity.Contracts.Queries;
using Conduit.Identity.Domain.Services;
using Conduit.Shared.Application.Cqrs;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Identity.Application;

public static class IdentityApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddCqrsMediator();

        services.AddScoped<ICommandHandler<RegisterUserCommand, string>, RegisterUserHandler>();
        services.AddScoped<ICommandHandler<AuthenticateUserCommand, string>, AuthenticateUserHandler>();
        services.AddScoped<ICommandHandler<FollowUserCommand>, FollowUserHandler>();
        services.AddScoped<ICommandHandler<UnfollowUserCommand>, UnfollowUserHandler>();
        services.AddScoped<ICommandHandler<UpdateUserCommand>, UpdateUserHandler>();

        services.AddScoped<IQueryHandler<CurrentUserQuery, User>, CurrentUserHandler>();
        services.AddScoped<IQueryHandler<ProfileQuery, Profile>, ProfileHandler>();
        services.AddScoped<IProfileQueryService, ProfileQueryService>();

        services.AddScoped<UniqueUserEmailValidator>();
        services.AddScoped<UniqueUsernameValidator>();
        services.AddScoped<UserLoginValidator>();

        return services;
    }
}
