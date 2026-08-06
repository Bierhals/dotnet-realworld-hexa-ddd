using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Articles.Domain.Rules;

public sealed class AuthorUsernameIsValid(string username) : IBusinessRule
{
    public const int MaximumLength = 255;

    public ErrorOr<Success> Check()
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Error.Validation("Article.AuthorRequired", "An author username must not be empty.");
        }

        if (username.Length > MaximumLength)
        {
            return Error.Validation("Article.AuthorTooLong", $"An author username must not be longer than {MaximumLength} characters.");
        }

        return Result.Success;
    }
}
