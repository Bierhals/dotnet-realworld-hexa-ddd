using System;
using Conduit.Shared.Application.EventHandling;
using Conduit.Shared.Infrastructure;
using Conduit.Shared.Infrastructure.Outbox;
using Conduit.Tags.Core.Application;
using Conduit.Tags.Core.Domain;
using Conduit.Tags.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Conduit.Tags.Core.Infrastructure;

public static class TagsPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddTagsPersistence(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        services.AddDbContext<TagsDbContext>((sp, options) =>
        {
            configureDbContext(options);
            options.AddInterceptors(new DispatchDomainEventsInterceptor(
                sp.GetRequiredService<ILogger<DispatchDomainEventsInterceptor>>(),
                sp.GetRequiredService<DomainEventDispatcher>()));
        });

        services.AddHostedService<OutboxProcessor<TagsDbContext>>();

        services.AddScoped<ITagsRepository, TagsRepository>();
        services.AddScoped<ITagsReadRepository, TagsReadRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TagsDbContext>());

        return services;
    }
}
