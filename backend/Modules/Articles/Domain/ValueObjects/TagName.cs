using Conduit.Articles.Domain.Rules;
using ErrorOr;

namespace Conduit.Articles.Domain.ValueObjects;

/// <summary>
/// A tag name as an article uses it. The tag catalog itself is owned by the Tags module; this is
/// the Articles module's own copy of the concept, which is why it validates independently instead
/// of sharing a type across the module boundary.
/// </summary>
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
