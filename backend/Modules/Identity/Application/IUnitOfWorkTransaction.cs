using System;
using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Identity.Application;

public interface IUnitOfWorkTransaction : IDisposable, IAsyncDisposable
{
    public Task CommitAsync(CancellationToken ct);
    public Task RollbackAsync(CancellationToken ct);
}
