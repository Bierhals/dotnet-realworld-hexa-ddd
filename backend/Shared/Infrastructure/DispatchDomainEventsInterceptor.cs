using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.EventHandling;
using Conduit.Shared.Domain;
using Conduit.Shared.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Conduit.Shared.Infrastructure;

/// <summary>
/// Stages domain events as OutboxDomainEvent rows in the same SaveChangesAsync call/transaction as
/// the aggregate change that raised them, giving an atomic delivery guarantee. Each event fans out
/// into one row per registered handler (see DomainEventDispatcher.GetHandlerTypes), so a background
/// OutboxProcessor can later retry one handler's failed delivery without re-running or blocking any
/// other handler for the same event. Logging happens right here instead, synchronously alongside
/// the outbox write: it's diagnostic output tied to "this event was raised," not a durable side
/// effect that needs the outbox's retry/delivery guarantees, so there's no reason to make it wait
/// for the background processor's next poll.
/// </summary>
public sealed class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    // The LoggerMessage source generator requires a partial class, which SaveChangesInterceptor
    // subclasses can't be combined with here, so the delegate-based LoggerMessage.Define API is
    // used instead.
    private static readonly Action<ILogger, string, DateTime, object, Exception?> LogDomainEvent =
        LoggerMessage.Define<string, DateTime, object>(
            LogLevel.Information,
            new EventId(0, "DomainEventLogged"),
            "Domain event {EventType} occurred at {OccurredOnUtc}: {DomainEvent}");

    private readonly ILogger<DispatchDomainEventsInterceptor> _logger;
    private readonly DomainEventDispatcher _dispatcher;

    public DispatchDomainEventsInterceptor(
        ILogger<DispatchDomainEventsInterceptor> logger, DomainEventDispatcher dispatcher)
    {
        _logger = logger;
        _dispatcher = dispatcher;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null)
        {
            return new ValueTask<InterceptionResult<int>>(result);
        }

        var entitiesWithEvents = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToArray();

        foreach (var entity in entitiesWithEvents)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                LogDomainEvent(_logger, domainEvent.GetType().Name, domainEvent.OccurredOnUtc, domainEvent, null);

                foreach (var handlerType in _dispatcher.GetHandlerTypes(domainEvent.GetType()))
                {
                    context.Add(OutboxDomainEvent.FromDomainEvent(domainEvent, handlerType));
                }
            }

            entity.ClearDomainEvents();
        }

        return new ValueTask<InterceptionResult<int>>(result);
    }
}
