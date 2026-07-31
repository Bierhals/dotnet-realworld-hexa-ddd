using System.Data;
using Conduit.Host.WebApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Conduit.Host.WebApi.Infrastructure;

public class ConduitContext(DbContextOptions<ConduitContext> options) : DbContext(options)
{
    private IDbContextTransaction? _currentTransaction;

    public DbSet<Article> Articles { get; init; } = null!;
    public DbSet<Comment> Comments { get; init; } = null!;
    public DbSet<Tag> Tags { get; init; } = null!;
    public DbSet<ArticleTag> ArticleTags { get; init; } = null!;
    public DbSet<ArticleFavorite> ArticleFavorites { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArticleTag>(b =>
        {
            b.HasKey(t => new { t.ArticleId, t.TagId });

            b.HasOne(pt => pt.Article)
                .WithMany(p => p.ArticleTags)
                .HasForeignKey(pt => pt.ArticleId);

            b.HasOne(pt => pt.Tag).WithMany(t => t.ArticleTags).HasForeignKey(pt => pt.TagId);
        });

        modelBuilder.Entity<ArticleFavorite>(b =>
        {
            b.HasKey(t => new { t.ArticleId, t.Username });

            b.HasOne(pt => pt.Article)
                .WithMany(p => p.ArticleFavorites)
                .HasForeignKey(pt => pt.ArticleId);
        });
    }

    #region Transaction Handling
    public void BeginTransaction()
    {
        if (_currentTransaction != null)
        {
            return;
        }

        if (!Database.IsInMemory())
        {
            _currentTransaction = Database.BeginTransaction(IsolationLevel.ReadCommitted);
        }
    }

    public void CommitTransaction()
    {
        try
        {
            _currentTransaction?.Commit();
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public void RollbackTransaction()
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }
    #endregion
}
