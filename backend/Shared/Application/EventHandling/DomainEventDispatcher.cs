using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Shared.Application.EventHandling;

public sealed class DomainEventDispatcher
{
    private readonly IServiceProvider _sp;
    public DomainEventDispatcher(IServiceProvider sp) => _sp = sp;

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
}
