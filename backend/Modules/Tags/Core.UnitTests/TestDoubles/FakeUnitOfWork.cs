using System.Threading;
using System.Threading.Tasks;
using Conduit.Tags.Core.Application;

namespace Conduit.Tags.Core.UnitTests.TestDoubles;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;

        return Task.CompletedTask;
    }
}
