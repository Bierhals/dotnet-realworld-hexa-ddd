using ErrorOr;

namespace Conduit.Identity.Domain.ValueObjects;

public sealed record UserImage
{
    public string Value { get; }
    private UserImage(string value) => Value = value;

    public static ErrorOr<UserImage> Create(string value)
    {
        // TODO: Validate the user image format
        return new UserImage(value.Trim().ToLowerInvariant());
    }

    public static UserImage Rehydrate(string value) => new(value);
}