using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Identity.Domain.Rules;

public sealed class UserImageUrlLengthIsInRange(string url) : IBusinessRule
{
    public const int MaximumLength = 2048;

    public ErrorOr<Success> Check()
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return Error.Validation("UserImage.UrlRequired", "A user image URL must not be empty.");
        }

        if (url.Length > MaximumLength)
        {
            return Error.Validation("UserImage.UrlTooLong", $"A user image URL must not be longer than {MaximumLength} characters.");
        }

        return Result.Success;
    }
}
