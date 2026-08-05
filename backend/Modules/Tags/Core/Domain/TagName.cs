using Conduit.Tags.Core.Domain.Rules;
using ErrorOr;

namespace Conduit.Tags.Core.Domain;

public sealed record TagName
{
    public string Value { get; }

    private TagName(string value) => Value = value;

    public static ErrorOr<TagName> Create(string value)
    {
        var sanitizedValue = value?.Trim() ?? string.Empty;

        var check = new TagNameIsValid(sanitizedValue).Check();

        return check.IsError ? check.Errors : new TagName(sanitizedValue);
    }

    public static TagName Rehydrate(string value) => new(value);
}
