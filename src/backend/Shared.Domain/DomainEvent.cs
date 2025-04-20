using System;
using System.Collections.Generic;

namespace Conduit.Shared.Domain;

public abstract class DomainEvent : ValueObject
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }

    public DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.Now;
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return OccurredOn;
        yield return Id;
    }
}
