using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application;
using Microsoft.EntityFrameworkCore.Storage;

namespace Conduit.Identity.Infrastructure.Persistence;

internal sealed class IdentityDbTransaction(IDbContextTransaction transaction) : IUnitOfWorkTransaction
{
    public Task CommitAsync(CancellationToken ct) => transaction.CommitAsync(ct);

    public Task RollbackAsync(CancellationToken ct) => transaction.RollbackAsync(ct);

    public void Dispose() => transaction.Dispose();

    public ValueTask DisposeAsync() => transaction.DisposeAsync();
}
