using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Tags.Core.Domain;

namespace Conduit.Tags.Core.UnitTests.TestDoubles;

internal sealed class FakeArticleTagUsageRepository : IArticleTagUsageRepository
{
    private readonly List<ArticleTagUsage> _usages = [];

    public IReadOnlyCollection<ArticleTagUsage> Usages => _usages;

    public ArticleTagUsage? Find(Guid articleId) => _usages.SingleOrDefault(usage => usage.Id == articleId);

    public Task<ArticleTagUsage?> GetAsync(Guid articleId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Find(articleId));

    public void Add(ArticleTagUsage usage) => _usages.Add(usage);
}
