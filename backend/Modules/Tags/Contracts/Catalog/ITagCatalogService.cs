using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace Conduit.Tags.Contracts.Catalog;

/// <summary>
/// The Tags module's public write surface. The tag catalog counts how many articles reference
/// each tag, so every consumer that starts using a tag must reference it and every consumer that
/// stops using it must release it again. A tag that loses its last reference leaves the catalog.
/// </summary>
public interface ITagCatalogService
{
    public Task<ErrorOr<Success>> ReferenceTagsAsync(IReadOnlyCollection<string> tagNames, CancellationToken cancellationToken = default);

    public Task<ErrorOr<Success>> ReleaseTagsAsync(IReadOnlyCollection<string> tagNames, CancellationToken cancellationToken = default);
}
