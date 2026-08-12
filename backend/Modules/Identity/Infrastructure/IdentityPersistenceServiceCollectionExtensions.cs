using System;
using Conduit.Identity.Application;
using Conduit.Identity.Domain;
using Conduit.Identity.Infrastructure.Persistence;
using Conduit.Shared.Application.EventHandling;
using Conduit.Shared.Infrastructure;
using Conduit.Shared.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Conduit.Identity.Infrastructure;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityPersistence(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            configureDbContext(options);
            options.AddInterceptors(new DispatchDomainEventsInterceptor(
                sp.GetRequiredService<ILogger<DispatchDomainEventsInterceptor>>(),
                sp.GetRequiredService<DomainEventDispatcher>()));
        });

        services.AddHostedService<OutboxProcessor<IdentityDbContext>>();

        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IUserFollowsRepository, UserFollowsRepository>();
        services.AddScoped<IUsersReadRepository, UsersReadRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IdentityDbContext>());

        return services;
    }
}
