using System.Linq;
using ErrorOr;

namespace Conduit.Shared.Domain.BuildingBlocks;

public static class BusinessRuleChecker
{
    public static ErrorOr<Success> Check(params IBusinessRule[] rules)
    {
        var errors = rules
            .Select(r => r.Check())
            .Where(r => r.IsError)
            .SelectMany(r => r.Errors)
            .ToList();

        return errors.Count != 0
            ? errors
            : Result.Success;
    }
}
