using Conduit.Articles.Domain.Rules;
using ErrorOr;

namespace Conduit.Articles.Domain.ValueObjects;

/// <summary>
/// The username of the account that authored or favorited something. Accounts are owned by the
/// Identity module, so this only carries the identifier - never any profile data.
/// </summary>
public sealed record AuthorUsername
{
    public string Value { get; }

    private AuthorUsername(string value) => Value = value;

    public static ErrorOr<AuthorUsername> Create(string value)
    {
        var sanitizedValue = value?.Trim() ?? string.Empty;

        var check = new AuthorUsernameIsValid(sanitizedValue).Check();

        return check.IsError ? check.Errors : new AuthorUsername(sanitizedValue);
    }

    public static AuthorUsername Rehydrate(string value) => new(value);
}
