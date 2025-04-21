using System;
using System.Collections.Generic;
using System.Linq;

namespace Conduit.Shared.Domain.BuildingBlocks;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        return obj is ValueObject other && Equals(other);
    }

    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(0, (x, y) => x ^ (y?.GetHashCode() ?? 0));
    }

    public static bool operator ==(ValueObject left, ValueObject right) => Equals(left, right);
    public static bool operator !=(ValueObject left, ValueObject right) => !Equals(left, right);
}
