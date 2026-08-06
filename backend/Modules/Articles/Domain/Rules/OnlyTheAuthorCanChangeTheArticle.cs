using Conduit.Articles.Domain.ValueObjects;
using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Articles.Domain.Rules;

public sealed class OnlyTheAuthorCanChangeTheArticle(AuthorUsername author, AuthorUsername requester) : IBusinessRule
{
    public ErrorOr<Success> Check() =>
        author == requester
            ? Result.Success
            : Error.Forbidden("Article.NotTheAuthor", "Only the author of an article can change or delete it.");
}
