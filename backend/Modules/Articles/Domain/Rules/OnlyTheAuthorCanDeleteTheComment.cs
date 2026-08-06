using Conduit.Articles.Domain.ValueObjects;
using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Articles.Domain.Rules;

public sealed class OnlyTheAuthorCanDeleteTheComment(AuthorUsername author, AuthorUsername requester) : IBusinessRule
{
    public ErrorOr<Success> Check() =>
        author == requester
            ? Result.Success
            : Error.Forbidden("Comment.NotTheAuthor", "Only the author of a comment can delete it.");
}
