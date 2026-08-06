using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Articles.Domain.Rules;

public sealed class ArticleTitleLengthIsInRange(string title) : IBusinessRule
{
    public const int MaximumLength = 255;

    public ErrorOr<Success> Check()
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Error.Validation("Article.TitleRequired", "An article title must not be empty.");
        }

        if (title.Length > MaximumLength)
        {
            return Error.Validation("Article.TitleTooLong", $"An article title must not be longer than {MaximumLength} characters.");
        }

        return Result.Success;
    }
}
