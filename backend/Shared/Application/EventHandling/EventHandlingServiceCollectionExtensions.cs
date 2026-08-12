using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Conduit.Shared.Application.EventHandling;

public static class EventHandlingServiceCollectionExtensions
{
    /// <summary>
    /// Registers DomainEventDispatcher (used by the OutboxProcessor to invoke handlers for
    /// processed outbox rows) and IDomainEventPublisher (the synchronous, pre-response escape
    /// hatch for Application handlers). Call once from the composition root.
    /// </summary>
    public static IServiceCollection AddDomainEventDispatching(this IServiceCollection services)
    {
        services.TryAddScoped<DomainEventDispatcher>();
        services.TryAddScoped<IDomainEventPublisher, DomainEventPublisher>();

        return services;
    }
}
