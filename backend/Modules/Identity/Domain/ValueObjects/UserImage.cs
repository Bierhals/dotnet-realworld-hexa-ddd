using Conduit.Identity.Domain.Rules;
using ErrorOr;

namespace Conduit.Identity.Domain.ValueObjects;

public sealed record UserImage
{
    public string Value { get; }
    private UserImage(string value) => Value = value;

    public static ErrorOr<UserImage> Create(string value)
    {
        var sanitizedValue = value.Trim();

        var check = new UserImageIsValidUrl(sanitizedValue).Check();

        return check.IsError ? check.Errors : new UserImage(sanitizedValue);
    }

    public static UserImage Rehydrate(string value) => new(value);
}