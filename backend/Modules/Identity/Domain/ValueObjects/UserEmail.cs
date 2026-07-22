using ErrorOr;

namespace Conduit.Identity.Domain.ValueObjects;

public sealed record UserEmail
{
    public string Value { get; }
    private UserEmail(string value) => Value = value;

    public static ErrorOr<UserEmail> Create(string value)
    {
        // TODO: Validate the email format
        return new UserEmail(value.Trim().ToLowerInvariant());
    }

    public static UserEmail Rehydrate(string value) => new(value);
}
