using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Domain;

namespace Conduit.Shared.Application.EventHandling;

public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent
{
    public Task Handle(TEvent domainEvent, CancellationToken ct);
}
