using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application;
using Conduit.Identity.Domain;
using Conduit.Shared.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserFollow> UserFollows => Set<UserFollow>();
    public DbSet<OutboxDomainEvent> OutboxDomainEvents => Set<OutboxDomainEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Identity");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        modelBuilder.Entity<OutboxDomainEvent>(builder =>
            OutboxDomainEventConfiguration.Configure(builder, Database.ProviderName, "Identity"));
    }

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct)
    {
        var transaction = await Database.BeginTransactionAsync(ct);
        return new IdentityDbTransaction(transaction);
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken ct) => await SaveChangesAsync(ct);
}
