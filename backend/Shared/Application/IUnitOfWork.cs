using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Shared.Application;

public interface IUnitOfWork
{
    public Task BeginTransactionAsync(CancellationToken ct);
    public Task SaveChangesAsync(CancellationToken ct);
    public Task CommitAsync(CancellationToken ct);
    public Task RollbackAsync(CancellationToken ct);
}
