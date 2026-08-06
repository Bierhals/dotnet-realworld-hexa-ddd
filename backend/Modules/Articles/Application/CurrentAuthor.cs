using Conduit.Articles.Domain.ValueObjects;
using Conduit.Shared.Application;
using ErrorOr;

namespace Conduit.Articles.Application;

internal static class CurrentAuthor
{
    public static ErrorOr<AuthorUsername> Resolve(ICurrentUserAccessor currentUserAccessor) =>
        currentUserAccessor.GetCurrentUsername() is { } username
            ? AuthorUsername.Create(username)
            : Error.Unauthorized("Article.NotAuthenticated", "This action requires an authenticated user.");
}
