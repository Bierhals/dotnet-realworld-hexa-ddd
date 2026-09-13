using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Infrastructure.Messaging;
using Conduit.Tags.Core.Application;
using Microsoft.Extensions.Logging;
using Wolverine.EntityFrameworkCore;

namespace Conduit.Tags.Core.Infrastructure.Persistence;

/// <summary>
/// Saves through Wolverine's outbox so that the tag change and the domain events it raised commit
/// together. The DbContext itself no longer implements IUnitOfWork: Wolverine has to own the save
/// call, and injecting the outbox into the context would be a dependency cycle.
/// </summary>
internal sealed class TagsUnitOfWork(
    TagsDbContext context,
    IDbContextOutbox<TagsDbContext> outbox,
    ILogger<TagsUnitOfWork> logger) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        outbox.SaveChangesAndPublishDomainEventsAsync(context, logger, cancellationToken);
}
