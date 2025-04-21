using System.Threading.Tasks;

namespace Conduit.Shared.Domain.BuildingBlocks;

public interface IUnitOfWork
{
    Task<int> CommitAsync();
    void Rollback();
}
