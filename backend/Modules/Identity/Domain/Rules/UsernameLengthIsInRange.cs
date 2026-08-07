using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Identity.Domain.Rules;

public sealed class UsernameLengthIsInRange(string username) : IBusinessRule
{
    public const int MaximumLength = 255;

    public ErrorOr<Success> Check()
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Error.Validation("User.UsernameRequired", "A username must not be empty.");
        }

        if (username.Length > MaximumLength)
        {
            return Error.Validation("User.UsernameTooLong", $"A username must not be longer than {MaximumLength} characters.");
        }

        return Result.Success;
    }
}
