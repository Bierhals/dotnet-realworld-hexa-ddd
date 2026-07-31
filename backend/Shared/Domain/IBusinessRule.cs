using ErrorOr;

namespace Conduit.Shared.Domain;

public interface IBusinessRule
{
    public ErrorOr<Success> Check();
}
