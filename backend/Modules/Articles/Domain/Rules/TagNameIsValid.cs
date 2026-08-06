using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Articles.Domain.Rules;

public sealed class TagNameIsValid(string tagName) : IBusinessRule
{
    public const int MaximumLength = 64;

    public ErrorOr<Success> Check()
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            return Error.Validation("Article.TagNameRequired", "A tag name must not be empty.");
        }

        if (tagName.Length > MaximumLength)
        {
            return Error.Validation("Article.TagNameTooLong", $"A tag name must not be longer than {MaximumLength} characters.");
        }

        return Result.Success;
    }
}
