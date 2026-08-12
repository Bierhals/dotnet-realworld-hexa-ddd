using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Shared.Application.EventHandling;

public sealed class DomainEventDispatcher
{
    private readonly IServiceProvider _sp;
    public DomainEventDispatcher(IServiceProvider sp) => _sp = sp;

    /// <summary>
    /// Invokes every handler registered for each event, in one call. Used by the synchronous
    /// IDomainEventPublisher path only - the outbox path dispatches per-handler instead (see
    /// DispatchToHandlerAsync), so a failing handler can never block or duplicate another.
    /// </summary>
    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct)
    {
        foreach (var domainEvent in events)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = _sp.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod("Handle")!;
                await (Task)method.Invoke(handler, [domainEvent, ct])!;
            }
        }
    }

    /// <summary>
    /// The concrete handler types currently registered for an event type. Used by
    /// DispatchDomainEventsInterceptor to fan a domain event out into one outbox row per handler,
    /// so each handler's delivery is tracked (and retried) independently of the others.
    /// </summary>
    public IEnumerable<Type> GetHandlerTypes(Type eventType)
    {
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        return _sp.GetServices(handlerType).Select(handler => handler!.GetType());
    }

    /// <summary>
    /// Invokes exactly one handler, by its concrete type, for one event. Used by OutboxProcessor:
    /// each outbox row now names the single handler it's for, so a failure in one handler never
    /// prevents or duplicates another handler's delivery of the same event.
    /// </summary>
    public async Task DispatchToHandlerAsync(IDomainEvent domainEvent, Type handlerType, CancellationToken ct)
    {
        var handlerInterfaceType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        var handler = _sp.GetServices(handlerInterfaceType).FirstOrDefault(h => h!.GetType() == handlerType)
            ?? throw new InvalidOperationException(
                $"Handler '{handlerType}' is no longer registered for event '{domainEvent.GetType()}'.");

        var method = handlerInterfaceType.GetMethod("Handle")!;
        await (Task)method.Invoke(handler, [domainEvent, ct])!;
    }
}
