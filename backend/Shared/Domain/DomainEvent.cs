using System;

namespace Conduit.Shared.Domain;

public abstract record DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
