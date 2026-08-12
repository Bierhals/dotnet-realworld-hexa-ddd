using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Domain;

namespace Conduit.Shared.Application.EventHandling;

/// <summary>
/// Synchronous, pre-response dispatch for the rare case where an Application handler needs a side
/// effect guaranteed to have run before the response is sent - not the default path. Most domain
/// events flow through the outbox (DispatchDomainEventsInterceptor + OutboxProcessor); use this
/// only when the outbox's eventual-consistency window is unacceptable for a specific handler.
/// </summary>
public interface IDomainEventPublisher
{
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
}
