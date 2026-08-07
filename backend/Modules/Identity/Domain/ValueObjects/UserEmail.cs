using Conduit.Identity.Domain.Rules;
using ErrorOr;

namespace Conduit.Identity.Domain.ValueObjects;

public sealed record UserEmail
{
    public string Value { get; }
    private UserEmail(string value) => Value = value;

    public static ErrorOr<UserEmail> Create(string value)
    {
        var sanitizedValue = value.Trim().ToLowerInvariant();

        return new UserEmailIsWellFormed(sanitizedValue).Check()
            .Then(_ => new UserEmailLengthIsInRange(sanitizedValue).Check())
            .Then(_ => new UserEmail(sanitizedValue));
    }

    public static UserEmail Rehydrate(string value) => new(value);
}
