using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Domain;

namespace Conduit.Shared.Application.EventHandling;

public sealed class DomainEventPublisher : IDomainEventPublisher
{
    private readonly DomainEventDispatcher _dispatcher;

    public DomainEventPublisher(DomainEventDispatcher dispatcher) => _dispatcher = dispatcher;

    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken) =>
        _dispatcher.DispatchAsync([domainEvent], cancellationToken);
}
