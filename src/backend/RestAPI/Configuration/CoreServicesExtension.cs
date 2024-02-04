using Conduit.Application.Common;
using Conduit.Application.Users.Repositories;
using Conduit.Application.Users.Services;
using Conduit.Domain.User;
using Conduit.Persistence;
using Conduit.Persistence.Users;
using Microsoft.Extensions.DependencyInjection;


namespace Conduit.RestAPI.Configuration;

static class CoreServicesExtension
{
    public static IServiceCollection AddConduitServices(this IServiceCollection services)
    {
        services.AddScoped<IUsersCounter, SqliteUsersCounter>();
        services.AddScoped<IUsersRepository, SqliteUsersRepository>();
        services.AddScoped<IUsersQueryRepository, SqliteUsersQueryRepository>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUnitOfWork, SqliteUnitOfWork>();

        return services;
    }
}
