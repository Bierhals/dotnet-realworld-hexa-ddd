using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Tags.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Tags.Core.Infrastructure.Persistence;

public sealed class ArticleTagUsageRepository(TagsDbContext dbContext) : IArticleTagUsageRepository
{
    public Task<ArticleTagUsage?> GetAsync(Guid articleId, CancellationToken cancellationToken = default) =>
        dbContext.ArticleTagUsages.FirstOrDefaultAsync(usage => usage.Id == articleId, cancellationToken);

    public void Add(ArticleTagUsage usage) => dbContext.ArticleTagUsages.Add(usage);
}
