using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Conduit.Shared.Application.EventHandling;

public static class EventHandlingServiceCollectionExtensions
{
    /// <summary>
    /// Registers a logging handler for every domain event type. Every module that dispatches
    /// domain events calls this from its own Add{Module}Application(); TryAddEnumerable keeps
    /// the registration idempotent no matter how many modules call it.
    /// </summary>
    public static IServiceCollection AddDomainEventLogging(this IServiceCollection services)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped(typeof(IDomainEventHandler<>), typeof(LoggingDomainEventHandler<>)));

        return services;
    }
}
