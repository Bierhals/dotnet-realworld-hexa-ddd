using System.Collections.Generic;

namespace Conduit.Shared.Domain;

public interface IAggregateRoot
{
    public IReadOnlyList<IDomainEvent> DomainEvents { get; }
    public void ClearDomainEvents();
}
