using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Conduit.Shared.Domain.BuildingBlocks;

public class DomainEventProcessor
{
    private readonly Dictionary<Type, List<Type>> _handlerTypes = new();
    private readonly IServiceProvider _serviceProvider;

    public DomainEventProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Register<TEvent, THandler>()
        where TEvent : DomainEvent
        where THandler : IDomainEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        if (!_handlerTypes.ContainsKey(eventType))
        {
            _handlerTypes[eventType] = new List<Type>();
        }

        _handlerTypes[eventType].Add(handlerType);
    }

    public async Task ProcessEvents(IAggregateRoot aggregate)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            var eventType = domainEvent.GetType();

            if (_handlerTypes.TryGetValue(eventType, out var value))
            {
                foreach (var handlerType in value)
                {
                    var handlerInstance = _serviceProvider.GetService(handlerType);

                    if (handlerInstance != null)
                    {
                        var handlerInterface = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
                        var handleMethod = handlerInterface.GetMethod("Handle");
                        if (handleMethod != null)
                        {
                            await (Task)handleMethod.Invoke(handlerInstance, [domainEvent])!;
                        }
                    }
                }
            }
        }

        aggregate.ClearDomainEvents();
    }
}
