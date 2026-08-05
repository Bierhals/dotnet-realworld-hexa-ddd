using System.Threading;
using System.Threading.Tasks;
using Conduit.Tags.Core.Application;
using Conduit.Tags.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Tags.Core.Infrastructure.Persistence;

public sealed class TagsDbContext(DbContextOptions<TagsDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Tags");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TagsDbContext).Assembly);
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        await SaveChangesAsync(cancellationToken);
}
