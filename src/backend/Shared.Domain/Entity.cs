using System;

namespace Conduit.Shared.Domain;

public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    public TId Id { get; }

    protected Entity()
    {
        Id = default!;
    }

    protected Entity(TId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> other && Equals(other);
    }

    public bool Equals(Entity<TId>? other)
    {
        if (Id is null || Id.Equals(default(TId)) || other is null || other.GetType() != GetType())
            return false;
        if (ReferenceEquals(this, other)) return true;

        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id?.GetHashCode() ?? GetHashCode();

    public static bool operator ==(Entity<TId> left, Entity<TId> right) => Equals(left, right);
    public static bool operator !=(Entity<TId> left, Entity<TId> right) => !Equals(left, right);
}
