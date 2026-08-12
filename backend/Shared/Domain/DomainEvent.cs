using System;

namespace Conduit.Shared.Domain;

public abstract record DomainEvent : IDomainEvent
{
    // init, not a plain getter: get-only properties can't be set by System.Text.Json on
    // deserialization, so the outbox's round-trip would silently replace these with a fresh
    // Guid/DateTime.UtcNow instead of the original values raised at construction time.
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}
