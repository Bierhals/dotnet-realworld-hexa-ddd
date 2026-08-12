using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.EventHandling;
using Conduit.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Conduit.Shared.Infrastructure.Outbox;

/// <summary>
/// Polls TDbContext's OutboxDomainEvents table and dispatches each pending row to the one handler it
/// names, via DomainEventDispatcher.DispatchToHandlerAsync. Rows are already fanned out one per
/// handler at write time (see DispatchDomainEventsInterceptor), so a failure here only retries that
/// row's own handler - it never re-runs or blocks any other handler for the same event. One
/// instance is registered per module DbContext, since outbox rows live in separate schemas/contexts
/// - this keeps each module's processor scoped to its own DbContext.
/// </summary>
public sealed class OutboxProcessor<TDbContext> : BackgroundService
    where TDbContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OutboxProcessorOptions _options;
    private readonly ILogger<OutboxProcessor<TDbContext>> _logger;

    // Identifies this processor instance for claim ownership - stable for the process's lifetime,
    // so concurrent replicas polling the same table never both claim the same row (see
    // ProcessBatchAsync).
    private readonly Guid _instanceId = Guid.NewGuid();

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxProcessorOptions> options,
        ILogger<OutboxProcessor<TDbContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.PollingInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // A whole-batch failure (e.g. transient DB unavailability) must not kill the loop -
                // the next tick retries.
                _logger.LogError(ex, "Outbox poll cycle for {DbContext} failed", typeof(TDbContext).Name);
            }
        }
    }

    internal async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<DomainEventDispatcher>();

        var pending = await ClaimBatchAsync(dbContext, ct);
        if (pending.Count == 0)
        {
            return;
        }

        foreach (var message in pending)
        {
            // A link, not a parent: dispatch happens asynchronously and possibly much later, so
            // nesting it under the original request's span would misrepresent timing. The link
            // still lets a trace viewer navigate from the request that raised the event to the
            // background dispatch it eventually caused, and back.
            using var activity = OutboxActivitySource.Instance.StartActivity(
                "outbox.dispatch",
                ActivityKind.Consumer,
                parentContext: default,
                links: BuildLinks(message.TraceParent));
            activity?.SetTag("outbox.message_id", message.Id);
            activity?.SetTag("outbox.event_type", message.DomainEventType);
            activity?.SetTag("outbox.handler_type", message.HandlerType);

            try
            {
                var eventType = Type.GetType(message.DomainEventType)
                    ?? throw new InvalidOperationException($"Cannot resolve outbox event type '{message.DomainEventType}'.");
                var handlerType = Type.GetType(message.HandlerType)
                    ?? throw new InvalidOperationException($"Cannot resolve outbox handler type '{message.HandlerType}'.");
                var domainEvent = (IDomainEvent)JsonSerializer.Deserialize(message.Payload, eventType)!;

                await dispatcher.DispatchToHandlerAsync(domainEvent, handlerType, ct);

                message.ProcessedOnUtc = DateTime.UtcNow;
                message.LastError = null;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                message.RetryCount++;
                message.LastError = ex.Message;

                // Release the claim immediately rather than waiting out the full lease, so a
                // failed message is eligible for retry on the very next poll cycle.
                message.ClaimedBy = null;
                message.ClaimedAtUtc = null;

                _logger.LogWarning(
                    ex,
                    "Failed to dispatch outbox message {MessageId} for handler {HandlerType} ({EventType}), attempt {RetryCount}/{MaxRetries}",
                    message.Id,
                    message.HandlerType,
                    message.DomainEventType,
                    message.RetryCount,
                    _options.MaxRetries);
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Atomically claims a batch of rows for this instance via a single UPDATE, then reads back
    /// exactly what it claimed. The UPDATE's own row locking is what makes this safe under
    /// concurrent replicas: if two instances race, the DB serializes the two UPDATEs, and whichever
    /// runs second no longer sees the already-claimed rows as matching its WHERE clause - so the
    /// same row is never claimed (and dispatched) by more than one instance at once. A stale claim
    /// from a crashed instance becomes claimable again once ClaimLeaseDuration elapses.
    /// </summary>
    private async Task<List<OutboxDomainEvent>> ClaimBatchAsync(TDbContext dbContext, CancellationToken ct)
    {
        var claimedAtUtc = DateTime.UtcNow;
        var leaseExpiresBefore = claimedAtUtc - _options.ClaimLeaseDuration;

        var claimedCount = await dbContext.Set<OutboxDomainEvent>()
            .Where(m => m.ProcessedOnUtc == null
                && m.RetryCount < _options.MaxRetries
                && (m.ClaimedAtUtc == null || m.ClaimedAtUtc < leaseExpiresBefore))
            .OrderBy(m => m.OccurredOnUtc)
            .Take(_options.BatchSize)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(m => m.ClaimedBy, _instanceId)
                    .SetProperty(m => m.ClaimedAtUtc, claimedAtUtc),
                ct);

        if (claimedCount == 0)
        {
            return [];
        }

        return await dbContext.Set<OutboxDomainEvent>()
            .Where(m => m.ClaimedBy == _instanceId && m.ClaimedAtUtc == claimedAtUtc && m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .ToListAsync(ct);
    }

    private static IEnumerable<ActivityLink>? BuildLinks(string? traceParent) =>
        traceParent is not null && ActivityContext.TryParse(traceParent, traceState: null, out var context)
            ? [new ActivityLink(context)]
            : null;
}
