using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Identity.Application.UnitTests.TestDoubles;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct) =>
        throw new System.NotSupportedException("No handler under test opens a transaction.");

    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}
