using ErrorOr;

namespace Conduit.Shared.Domain.BuildingBlocks;

public interface IBusinessRule
{
    ErrorOr<Success> Check();
}
