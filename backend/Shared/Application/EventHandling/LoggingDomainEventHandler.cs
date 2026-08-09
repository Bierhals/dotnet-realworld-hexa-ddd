using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Domain;
using Microsoft.Extensions.Logging;

namespace Conduit.Shared.Application.EventHandling;

public sealed class LoggingDomainEventHandler<TEvent> : IDomainEventHandler<TEvent>
    where TEvent : IDomainEvent
{
    // The LoggerMessage source generator doesn't support generic containing types,
    // so the delegate-based LoggerMessage.Define API is used instead.
    private static readonly Action<ILogger, string, DateTime, TEvent, Exception?> LogDomainEvent =
        LoggerMessage.Define<string, DateTime, TEvent>(
            LogLevel.Information,
            new EventId(0, "DomainEventLogged"),
            "Domain event {EventType} occurred at {OccurredOnUtc}: {DomainEvent}");

    private readonly ILogger<LoggingDomainEventHandler<TEvent>> _logger;

    public LoggingDomainEventHandler(ILogger<LoggingDomainEventHandler<TEvent>> logger) => _logger = logger;

    public Task Handle(TEvent domainEvent, CancellationToken ct)
    {
        LogDomainEvent(_logger, typeof(TEvent).Name, domainEvent.OccurredOnUtc, domainEvent, null);

        return Task.CompletedTask;
    }
}
