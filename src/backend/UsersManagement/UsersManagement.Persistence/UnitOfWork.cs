using System.Threading.Tasks;
using Conduit.Shared.Domain.BuildingBlocks;
//using Conduit.Domain.Common;

namespace Conduit.UsersManagement.Persistence;

public class UnitOfWork : IUnitOfWork
{
    readonly UsersManagementContext _context;

    public UnitOfWork(UsersManagementContext context)
    {
        _context = context;
    }

    public Task<int> CommitAsync()
    {
        return _context.SaveChangesAsync();
    }

    public void Rollback()
    {
        _context.ChangeTracker.Clear();
    }
    
    /*public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // When should you publish domain events?
        //
        // 1. BEFORE calling SaveChangesAsync
        //     - domain events are part of the same transaction
        //     - immediate consistency
        // 2. AFTER calling SaveChangesAsync
        //     - domain events are a separate transaction
        //     - eventual consistency
        //     - handlers can fail
        await PublishNewDomainEvents(cancellationToken);

        return await base.SaveChangesAsync(cancellationToken);
    }

    async Task PublishNewDomainEvents(CancellationToken cancellationToken)
    {
        IDomainEvent[] newDomainEvents = GetNewDomainEvents();

        foreach (IDomainEvent domainEvent in newDomainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }

    IDomainEvent[] GetNewDomainEvents()
    {
        return ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                IReadOnlyCollection<IDomainEvent> domainEvents = entity.DomainEvents;

                entity.ClearDomainEvents();

                return domainEvents;
            })
            .ToArray();
    }*/
}
