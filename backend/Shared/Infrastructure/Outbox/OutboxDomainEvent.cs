using System;
using System.Diagnostics;
using System.Text.Json;
using Conduit.Shared.Domain;

namespace Conduit.Shared.Infrastructure.Outbox;

/// <summary>
/// A row written atomically with the aggregate change that raised the domain event it carries.
/// Plain persistence record, not a domain type - Domain must stay persistence-free, so this lives
/// in Infrastructure even though it's Shared.
/// </summary>
public sealed class OutboxDomainEvent
{
    public required Guid Id { get; init; }

    /// <summary>
    /// The originating domain event's own id. One domain event fans out into one row per handler
    /// (see FromDomainEvent), so this - not Id - is what correlates those rows back to "the same
    /// event happened."
    /// </summary>
    public required Guid DomainEventId { get; init; }

    /// <summary>
    /// Full CLR type name of the serialized event (e.g. "Conduit.Articles.Domain.Events.
    /// ArticlePublishedDomainEvent, Conduit.Articles.Domain"), used to deserialize back to
    /// IDomainEvent at dispatch time.
    /// </summary>
    public required string DomainEventType { get; init; }

    /// <summary>
    /// Full CLR type name of the one handler this row is for (same format as DomainEventType).
    /// Each handler registered for an event gets its own row, so one handler's failure/retry never
    /// blocks or duplicates another handler's delivery of the same event.
    /// </summary>
    public required string HandlerType { get; init; }

    public required string Payload { get; init; }

    public required DateTime OccurredOnUtc { get; init; }

    public DateTime? ProcessedOnUtc { get; set; }

    public string? LastError { get; set; }

    public int RetryCount { get; set; }

    /// <summary>
    /// Which OutboxProcessor instance currently owns this row, so that concurrent instances
    /// (multiple replicas polling the same table) don't dispatch the same event twice. Claims are
    /// leased, not permanent - see OutboxProcessorOptions.ClaimLeaseDuration - so a crashed
    /// instance's claims become eligible for re-claiming instead of blocking the row forever.
    /// </summary>
    public Guid? ClaimedBy { get; set; }

    public DateTime? ClaimedAtUtc { get; set; }

    /// <summary>
    /// The W3C traceparent of the activity that raised this event (if any was active), captured at
    /// write time so OutboxProcessor can later link its dispatch span back to the request that
    /// caused it, even though dispatch happens asynchronously and potentially much later.
    /// </summary>
    public string? TraceParent { get; init; }

    public static OutboxDomainEvent FromDomainEvent(IDomainEvent domainEvent, Type handlerType)
    {
        var eventType = domainEvent.GetType();

        return new OutboxDomainEvent
        {
            Id = Guid.NewGuid(),
            DomainEventId = domainEvent.Id,
            DomainEventType = $"{eventType.FullName}, {eventType.Assembly.GetName().Name}",
            HandlerType = $"{handlerType.FullName}, {handlerType.Assembly.GetName().Name}",
            Payload = JsonSerializer.Serialize(domainEvent, eventType),
            OccurredOnUtc = domainEvent.OccurredOnUtc,
            TraceParent = Activity.Current?.Id,
        };
    }
}
