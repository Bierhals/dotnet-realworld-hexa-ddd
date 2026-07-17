using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.EventHandling;
using Conduit.Shared.Domain;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Conduit.Shared.Infrastructure;

public sealed class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly DomainEventDispatcher _domainEventDispatcher;

    public DispatchDomainEventsInterceptor(DomainEventDispatcher domainEventDispatcher)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    // TODO: Implement outbox pattern to resiliently handle domain events in case of failures
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null)
        {
            return result;
        }

        var entitiesWithEvents = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToArray();

        await _domainEventDispatcher.DispatchAsync([.. entitiesWithEvents.SelectMany(e => e.DomainEvents)], cancellationToken);
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        return result;
    }
}
