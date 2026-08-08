using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application;

namespace Conduit.Articles.Application.UnitTests.TestDoubles;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;

        return Task.CompletedTask;
    }
}
