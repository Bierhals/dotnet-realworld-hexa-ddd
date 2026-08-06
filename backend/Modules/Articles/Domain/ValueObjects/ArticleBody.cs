using Conduit.Articles.Domain.Rules;
using ErrorOr;

namespace Conduit.Articles.Domain.ValueObjects;

public sealed record ArticleBody
{
    public string Value { get; }

    private ArticleBody(string value) => Value = value;

    public static ErrorOr<ArticleBody> Create(string value)
    {
        var sanitizedValue = value ?? string.Empty;

        var check = new ArticleBodyIsValid(sanitizedValue).Check();

        return check.IsError ? check.Errors : new ArticleBody(sanitizedValue);
    }

    public static ArticleBody Rehydrate(string value) => new(value);
}
