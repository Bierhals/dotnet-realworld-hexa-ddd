using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application;
using Conduit.Tags.Contracts.Catalog;
using ErrorOr;

namespace Conduit.Articles.Infrastructure.Adapters;

/// <summary>
/// Adapts the Tags module's public catalog contract to the port the Articles module owns. This is
/// the only place in Articles that is allowed to know the Tags module exists.
/// </summary>
internal sealed class TagsCatalogAdapter(ITagCatalogService tagCatalogService) : ITagCatalog
{
    public Task<ErrorOr<Success>> ReferenceTagsAsync(
        IReadOnlyCollection<string> tagNames,
        CancellationToken cancellationToken = default) =>
        tagCatalogService.ReferenceTagsAsync(tagNames, cancellationToken);

    public Task<ErrorOr<Success>> ReleaseTagsAsync(
        IReadOnlyCollection<string> tagNames,
        CancellationToken cancellationToken = default) =>
        tagCatalogService.ReleaseTagsAsync(tagNames, cancellationToken);
}
