using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Identity.Application;

public interface IUnitOfWork
{
    public Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct);
    public Task SaveChangesAsync(CancellationToken ct);
}
