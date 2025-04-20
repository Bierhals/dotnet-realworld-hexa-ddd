using System.Threading.Tasks;

namespace Conduit.Shared.Domain;

public interface IUnitOfWork
{
    Task<int> CommitAsync();
    void Rollback();
}
