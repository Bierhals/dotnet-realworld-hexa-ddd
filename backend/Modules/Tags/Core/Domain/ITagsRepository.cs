using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Tags.Core.Domain;

public interface ITagsRepository
{
    public Task<IReadOnlyCollection<Tag>> GetByNamesAsync(IReadOnlyCollection<TagName> names, CancellationToken cancellationToken = default);

    public void Add(Tag tag);

    public void Remove(Tag tag);
}
