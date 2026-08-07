using Conduit.Articles.Domain.Rules;
using ErrorOr;

namespace Conduit.Articles.Domain.ValueObjects;

/// <summary>
/// The username of the account that authored or favorited something. Accounts are owned by the
/// Identity module, so this only carries the identifier - never any profile data.
/// </summary>
public sealed record Username
{
    public string Value { get; }

    private Username(string value) => Value = value;

    public static ErrorOr<Username> Create(string value)
    {
        var sanitizedValue = value?.Trim() ?? string.Empty;

        var check = new UsernameLengthIsInRange(sanitizedValue).Check();

        return check.IsError ? check.Errors : new Username(sanitizedValue);
    }

    public static Username Rehydrate(string value) => new(value);
}
