using Conduit.Articles.Domain.ValueObjects;
using Conduit.Shared.Application;
using ErrorOr;

namespace Conduit.Articles.Application;

internal static class CurrentUser
{
    public static ErrorOr<Username> Resolve(ICurrentUserAccessor currentUserAccessor) =>
        currentUserAccessor.GetCurrentUsername() is { } username
            ? Username.Create(username)
            : Error.Unauthorized("Article.NotAuthenticated", "This action requires an authenticated user.");
}
