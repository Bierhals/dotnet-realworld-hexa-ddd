using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Articles.Domain.Rules;

public sealed class ArticleDescriptionLengthIsInRange(string description) : IBusinessRule
{
    public const int MaximumLength = 1024;

    public ErrorOr<Success> Check()
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Error.Validation("Article.DescriptionRequired", "An article description must not be empty.");
        }

        if (description.Length > MaximumLength)
        {
            return Error.Validation("Article.DescriptionTooLong", $"An article description must not be longer than {MaximumLength} characters.");
        }

        return Result.Success;
    }
}
