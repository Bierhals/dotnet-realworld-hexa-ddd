using System.Collections.Generic;

namespace Conduit.Shared.Domain.BuildingBlocks;

public interface IAggregateRoot
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
