using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Tags.Core.Application;

public interface IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
