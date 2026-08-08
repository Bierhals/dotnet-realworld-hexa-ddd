using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace Conduit.Articles.Application;

/// <summary>
/// The tag catalog, which the Articles module does not own. An article never creates or deletes
/// tags itself - it only announces which tags it starts and stops using.
/// </summary>
public interface ITagCatalog
{
    public Task<ErrorOr<Success>> ReferenceTagsAsync(
        IReadOnlyCollection<string> tagNames,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<Success>> ReleaseTagsAsync(
        IReadOnlyCollection<string> tagNames,
        CancellationToken cancellationToken = default);
}
