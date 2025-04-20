using System;
using System.Collections.Generic;

namespace Conduit.Shared.Domain;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = [];

    protected AggregateRoot() : base() { }
    protected AggregateRoot(TId id) : base(id) { }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
