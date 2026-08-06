using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Articles.Domain.Rules;

public sealed class CommentBodyIsValid(string body) : IBusinessRule
{
    public ErrorOr<Success> Check() =>
        string.IsNullOrWhiteSpace(body)
            ? Error.Validation("Comment.BodyRequired", "A comment body must not be empty.")
            : Result.Success;
}
