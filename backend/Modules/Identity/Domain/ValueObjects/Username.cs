using Conduit.Identity.Domain.Rules;
using ErrorOr;

namespace Conduit.Identity.Domain.ValueObjects;

public sealed record Username
{
    public string Value { get; }
    private Username(string value) => Value = value;

    public static ErrorOr<Username> Create(string value)
    {
        var sanitizedValue = value.Trim();

        return new UsernameContainsOnlyAllowedCharacters(sanitizedValue).Check()
            .Then(_ => new UsernameLengthIsInRange(sanitizedValue).Check())
            .Then(_ => new Username(sanitizedValue));
    }

    public static Username Rehydrate(string value) => new(value);
}
