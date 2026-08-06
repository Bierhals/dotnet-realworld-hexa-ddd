using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application;
using Conduit.Articles.Domain;
using Conduit.Articles.Infrastructure.Persistence.CommentNumbers;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Articles.Infrastructure.Persistence;

public sealed class ArticlesDbContext(DbContextOptions<ArticlesDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public const string SchemaName = "Articles";

    /// <summary>
    /// The database object that hands out comment numbers. Providers that support sequences get a
    /// real sequence; the rest fall back to a counter table of the same name.
    /// </summary>
    public const string CommentNumbersName = "CommentNumbers";

    public DbSet<Article> Articles => Set<Article>();

    public DbSet<Comment> Comments => Set<Comment>();

    // Exposed for the read side only. Writes always go through the Article aggregate, which owns
    // these rows and keeps them consistent.
    public DbSet<ArticleTag> ArticleTags => Set<ArticleTag>();

    public DbSet<ArticleFavorite> ArticleFavorites => Set<ArticleFavorite>();

    /// <summary>
    /// EF Core models sequences only for providers that actually have them - SQLite, for instance,
    /// does not. Declaring one anyway would make table creation fail there.
    /// </summary>
    public static bool SupportsSequences(string? providerName) =>
        providerName is not null
        && !providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ArticlesDbContext).Assembly);

        if (SupportsSequences(Database.ProviderName))
        {
            modelBuilder.HasSequence<int>(CommentNumbersName, SchemaName).StartsAt(1).IncrementsBy(1);
        }
        else
        {
            modelBuilder.Entity<CommentNumberCounter>(builder =>
            {
                builder.ToTable(CommentNumbersName, SchemaName);
                builder.HasKey(counter => counter.Id);
                builder.Property(counter => counter.Id).ValueGeneratedNever();
                builder.Property(counter => counter.NextValue).IsRequired();
            });
        }
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        await SaveChangesAsync(cancellationToken);
}
