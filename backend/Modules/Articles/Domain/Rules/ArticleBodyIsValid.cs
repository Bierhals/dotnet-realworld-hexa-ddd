using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Articles.Domain.Rules;

public sealed class ArticleBodyIsValid(string body) : IBusinessRule
{
    public ErrorOr<Success> Check() =>
        string.IsNullOrWhiteSpace(body)
            ? Error.Validation("Article.BodyRequired", "An article body must not be empty.")
            : Result.Success;
}
