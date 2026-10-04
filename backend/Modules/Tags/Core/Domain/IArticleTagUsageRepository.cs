using System;
using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Tags.Core.Domain;

public interface IArticleTagUsageRepository
{
    public Task<ArticleTagUsage?> GetAsync(Guid articleId, CancellationToken cancellationToken = default);

    public void Add(ArticleTagUsage usage);
}
