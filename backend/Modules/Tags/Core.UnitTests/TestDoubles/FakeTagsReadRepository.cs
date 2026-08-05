using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Tags.Core.Application;

namespace Conduit.Tags.Core.UnitTests.TestDoubles;

internal sealed class FakeTagsReadRepository(params string[] tagNames) : ITagsReadRepository
{
    public Task<IReadOnlyCollection<string>> GetTagNamesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<string> names = [.. tagNames.OrderBy(name => name, System.StringComparer.Ordinal)];

        return Task.FromResult(names);
    }
}
