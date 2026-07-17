using System;

namespace Conduit.Shared.Domain;

public interface IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOnUtc { get; }
}
