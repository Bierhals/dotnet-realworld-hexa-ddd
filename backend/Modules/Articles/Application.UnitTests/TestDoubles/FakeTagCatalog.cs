using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application;
using ErrorOr;

namespace Conduit.Articles.Application.UnitTests.TestDoubles;

/// <summary>
/// Mirrors the reference counting the real Tags module does, so that tests can assert on the
/// catalog the way the module would end up looking.
/// </summary>
internal sealed class FakeTagCatalog : ITagCatalog
{
    private readonly Dictionary<string, int> _referenceCounts = [];

    public IReadOnlyCollection<string> Tags => _referenceCounts.Keys;

    /// <summary>
    /// Set to have the catalog reject the next reference, the way an unusable tag name would.
    /// </summary>
    public Error? RejectReferencesWith { get; set; }

    public int ReferenceCountOf(string tagName) =>
        _referenceCounts.TryGetValue(tagName, out var count) ? count : 0;

    /// <summary>
    /// Records one use of each name, for arranging a catalog that other articles already use.
    /// </summary>
    public void Seed(params string[] tagNames)
    {
        foreach (var tagName in tagNames)
        {
            _referenceCounts[tagName] = ReferenceCountOf(tagName) + 1;
        }
    }

    public Task<ErrorOr<Success>> ReferenceTagsAsync(
        IReadOnlyCollection<string> tagNames,
        CancellationToken cancellationToken = default)
    {
        if (RejectReferencesWith is { } error)
        {
            return Task.FromResult<ErrorOr<Success>>(error);
        }

        foreach (var tagName in tagNames)
        {
            _referenceCounts[tagName] = ReferenceCountOf(tagName) + 1;
        }

        return Task.FromResult<ErrorOr<Success>>(Result.Success);
    }

    public Task<ErrorOr<Success>> ReleaseTagsAsync(
        IReadOnlyCollection<string> tagNames,
        CancellationToken cancellationToken = default)
    {
        foreach (var tagName in tagNames)
        {
            var remaining = ReferenceCountOf(tagName) - 1;
            if (remaining > 0)
            {
                _referenceCounts[tagName] = remaining;
            }
            else
            {
                _referenceCounts.Remove(tagName);
            }
        }

        return Task.FromResult<ErrorOr<Success>>(Result.Success);
    }
}
