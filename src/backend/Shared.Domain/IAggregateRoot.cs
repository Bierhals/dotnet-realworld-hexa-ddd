using System.Collections.Generic;

namespace Conduit.Shared.Domain;

public interface IAggregateRoot
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
