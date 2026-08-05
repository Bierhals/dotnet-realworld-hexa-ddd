using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Tags.Contracts.Catalog;
using ErrorOr;

namespace Conduit.Host.WebApi.IntegrationTests;

/// <summary>
/// In-memory stand-in for the Tags module's real ITagCatalogService, used by SliceFixture-based
/// tests that never wire up the real Tags module. It mirrors the real catalog's behaviour: a tag
/// is counted per user and disappears once nothing references it anymore.
/// </summary>
public class FakeTagCatalogService : ITagCatalogService
{
    private readonly ConcurrentDictionary<string, int> _referenceCounts = new();

    public IReadOnlyCollection<string> Tags =>
        [.. _referenceCounts.Keys.OrderBy(name => name, System.StringComparer.Ordinal)];

    public int ReferenceCountOf(string tagName) =>
        _referenceCounts.TryGetValue(tagName, out var count) ? count : 0;

    public Task<ErrorOr<Success>> ReferenceTagsAsync(
        IReadOnlyCollection<string> tagNames,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var tagName in tagNames.Distinct(System.StringComparer.Ordinal))
        {
            _referenceCounts.AddOrUpdate(tagName, 1, (_, count) => count + 1);
        }

        return Task.FromResult<ErrorOr<Success>>(Result.Success);
    }

    public Task<ErrorOr<Success>> ReleaseTagsAsync(
        IReadOnlyCollection<string> tagNames,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var tagName in tagNames.Distinct(System.StringComparer.Ordinal))
        {
            if (!_referenceCounts.TryGetValue(tagName, out var count))
            {
                continue;
            }

            if (count <= 1)
            {
                _referenceCounts.TryRemove(tagName, out _);
            }
            else
            {
                _referenceCounts.TryUpdate(tagName, count - 1, count);
            }
        }

        return Task.FromResult<ErrorOr<Success>>(Result.Success);
    }
}
