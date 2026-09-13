using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application;
using Conduit.Shared.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;
using Wolverine.EntityFrameworkCore;

namespace Conduit.Articles.Infrastructure.Persistence;

/// <summary>
/// Saves through Wolverine's outbox so that the article change and the domain events it raised
/// commit together. The DbContext itself no longer implements IUnitOfWork: Wolverine has to own the
/// save call, and injecting the outbox into the context would be a dependency cycle.
/// </summary>
internal sealed class ArticlesUnitOfWork(
    ArticlesDbContext context,
    IDbContextOutbox<ArticlesDbContext> outbox,
    ILogger<ArticlesUnitOfWork> logger) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        outbox.SaveChangesAndPublishDomainEventsAsync(context, logger, cancellationToken);
}
