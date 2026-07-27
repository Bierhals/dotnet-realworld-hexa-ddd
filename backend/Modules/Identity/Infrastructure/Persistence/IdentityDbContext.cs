using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application;
using Conduit.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserFollow> UserFollows => Set<UserFollow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Identity");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct)
    {
        var transaction = await Database.BeginTransactionAsync(ct);
        return new IdentityDbTransaction(transaction);
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken ct) => await SaveChangesAsync(ct);
}
