using Conduit.Articles.Domain.Rules;
using ErrorOr;

namespace Conduit.Articles.Domain.ValueObjects;

public sealed record ArticleDescription
{
    public string Value { get; }

    private ArticleDescription(string value) => Value = value;

    public static ErrorOr<ArticleDescription> Create(string value)
    {
        var sanitizedValue = value?.Trim() ?? string.Empty;

        var check = new ArticleDescriptionIsValid(sanitizedValue).Check();

        return check.IsError ? check.Errors : new ArticleDescription(sanitizedValue);
    }

    public static ArticleDescription Rehydrate(string value) => new(value);
}
