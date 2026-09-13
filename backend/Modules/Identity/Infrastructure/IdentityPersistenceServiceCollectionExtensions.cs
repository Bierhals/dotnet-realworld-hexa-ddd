using System;
using Conduit.Identity.Application;
using Conduit.Identity.Domain;
using Conduit.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine.EntityFrameworkCore;

namespace Conduit.Identity.Infrastructure;

public static class IdentityPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityPersistence(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        // Registers the context and wires Wolverine's outbox into it, so that domain events can be
        // staged in the same transaction as the user change that raised them.
        services.AddDbContextWithWolverineIntegration<IdentityDbContext>(configureDbContext);

        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IUserFollowsRepository, UserFollowsRepository>();
        services.AddScoped<IUsersReadRepository, UsersReadRepository>();
        services.AddScoped<IUnitOfWork, IdentityUnitOfWork>();

        return services;
    }
}
