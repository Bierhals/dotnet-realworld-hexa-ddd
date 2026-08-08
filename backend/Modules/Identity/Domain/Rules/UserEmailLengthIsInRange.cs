using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Identity.Domain.Rules;

public sealed class UserEmailLengthIsInRange(string email) : IBusinessRule
{
    public const int MaximumLength = 254;

    public ErrorOr<Success> Check()
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Error.Validation("User.EmailRequired", "A user email must not be empty.");
        }

        if (email.Length > MaximumLength)
        {
            return Error.Validation("User.EmailTooLong", $"A user email must not be longer than {MaximumLength} characters.");
        }

        return Result.Success;
    }
}
