using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Tags.Core.Domain.Rules;

public sealed class TagMustBeReferenced(int referenceCount) : IBusinessRule
{
    public ErrorOr<Success> Check() =>
        referenceCount > 0
            ? Result.Success
            : Error.Validation("Tag.NotReferenced", "A tag that is not referenced by anything cannot be released.");
}
