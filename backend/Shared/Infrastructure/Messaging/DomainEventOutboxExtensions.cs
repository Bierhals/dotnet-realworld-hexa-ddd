using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine.EntityFrameworkCore;

namespace Conduit.Shared.Infrastructure.Messaging;

/// <summary>
/// The one place where an aggregate's domain events become messages. Every module's unit of work
/// routes its save through here.
/// </summary>
public static class DomainEventOutboxExtensions
{
    // The LoggerMessage source generator requires a partial class, which a static extension class
    // can't be combined with here, so the delegate-based LoggerMessage.Define API is used instead.
    private static readonly Action<ILogger, string, DateTime, object, Exception?> LogDomainEvent =
        LoggerMessage.Define<string, DateTime, object>(
            LogLevel.Information,
            new EventId(0, "DomainEventRaised"),
            "Domain event {EventType} occurred at {OccurredOnUtc}: {DomainEvent}");

    /// <summary>
    /// Collects the domain events raised by the tracked aggregates, hands them to Wolverine, and
    /// commits both in a single transaction.
    /// </summary>
    /// <remarks>
    /// Wolverine has to own the save: <c>SaveChangesAndFlushMessagesAsync</c> writes the entity
    /// changes and the outgoing message envelopes under one transaction and only then releases the
    /// messages for delivery. That is why this is a unit-of-work concern and not a
    /// <c>SaveChangesInterceptor</c> - an interceptor runs *inside* SaveChangesAsync and cannot
    /// wrap it.
    ///
    /// Delivery is at-least-once and unordered, so handlers must be idempotent.
    /// </remarks>
    public static async Task SaveChangesAndPublishDomainEventsAsync<TDbContext>(
        this IDbContextOutbox<TDbContext> outbox,
        TDbContext context,
        ILogger logger,
        CancellationToken cancellationToken)
        where TDbContext : DbContext
    {
        var aggregates = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToArray();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                LogDomainEvent(logger, domainEvent.GetType().Name, domainEvent.OccurredOnUtc, domainEvent, null);

                await outbox.PublishAsync(domainEvent);
            }

            // Cleared before the save so that a second SaveChangesAsync on the same tracked
            // aggregate cannot publish the same event twice.
            aggregate.ClearDomainEvents();
        }

        await outbox.SaveChangesAndFlushMessagesAsync(cancellationToken);
    }
}
