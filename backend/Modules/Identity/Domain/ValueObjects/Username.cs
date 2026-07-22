using ErrorOr;

namespace Conduit.Identity.Domain.ValueObjects;

public sealed record Username
{
    public string Value { get; }
    private Username(string value) => Value = value;

    public static ErrorOr<Username> Create(string value)
    {
        // TODO: Validate the username format
        return new Username(value.Trim().ToLowerInvariant());
    }

    public static Username Rehydrate(string value) => new(value);
}