using System.Linq;
using Conduit.Articles.Infrastructure.Persistence;
using Conduit.Articles.Infrastructure.Persistence.CommentNumbers;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Articles.Infrastructure;

public static class ArticlesModuleInitializer
{
    /// <summary>
    /// Prepares the module's storage once its tables exist. On providers with real sequences there
    /// is nothing to do; the counter table that stands in for them elsewhere needs its single row.
    /// </summary>
    public static void EnsureCommentNumbersReady(ArticlesDbContext dbContext)
    {
        if (ArticlesDbContext.SupportsSequences(dbContext.Database.ProviderName))
        {
            return;
        }

        var counters = dbContext.Set<CommentNumberCounter>();
        if (counters.Any(counter => counter.Id == CommentNumberCounter.SingletonId))
        {
            return;
        }

        // Starts at zero because the generator increments before it reads, so the first comment
        // gets number one.
        counters.Add(new CommentNumberCounter
        {
            Id = CommentNumberCounter.SingletonId,
            NextValue = 0,
        });

        dbContext.SaveChanges();
    }
}
