using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Infrastructure.Outbox;
using Conduit.Tags.Core.Application;
using Conduit.Tags.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Tags.Core.Infrastructure.Persistence;

public sealed class TagsDbContext(DbContextOptions<TagsDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<OutboxDomainEvent> OutboxDomainEvents => Set<OutboxDomainEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Tags");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TagsDbContext).Assembly);
        modelBuilder.Entity<OutboxDomainEvent>(builder =>
            OutboxDomainEventConfiguration.Configure(builder, Database.ProviderName, "Tags"));
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        await SaveChangesAsync(cancellationToken);
}
