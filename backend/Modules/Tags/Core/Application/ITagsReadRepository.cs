using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Tags.Core.Application;

public interface ITagsReadRepository
{
    public Task<IReadOnlyCollection<string>> GetTagNamesAsync(CancellationToken cancellationToken = default);
}
