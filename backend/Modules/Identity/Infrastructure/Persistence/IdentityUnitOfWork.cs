using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application;
using Conduit.Shared.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine.EntityFrameworkCore;

namespace Conduit.Identity.Infrastructure.Persistence;

/// <summary>
/// Saves through Wolverine's outbox so that the user change and the domain events it raised commit
/// together. The DbContext itself no longer implements IUnitOfWork: Wolverine has to own the save
/// call, and injecting the outbox into the context would be a dependency cycle.
/// </summary>
internal sealed class IdentityUnitOfWork(
    IdentityDbContext context,
    IDbContextOutbox<IdentityDbContext> outbox,
    ILogger<IdentityUnitOfWork> logger) : IUnitOfWork
{
    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct)
    {
        var transaction = await context.Database.BeginTransactionAsync(ct);

        return new IdentityDbTransaction(transaction);
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        outbox.SaveChangesAndPublishDomainEventsAsync(context, logger, ct);
}
