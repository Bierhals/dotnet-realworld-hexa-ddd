using Conduit.Articles.Domain.Rules;
using ErrorOr;

namespace Conduit.Articles.Domain.ValueObjects;

public sealed record ArticleTitle
{
    public string Value { get; }

    private ArticleTitle(string value) => Value = value;

    public static ErrorOr<ArticleTitle> Create(string value)
    {
        var sanitizedValue = value?.Trim() ?? string.Empty;

        var check = new ArticleTitleLengthIsInRange(sanitizedValue).Check();

        return check.IsError ? check.Errors : new ArticleTitle(sanitizedValue);
    }

    public static ArticleTitle Rehydrate(string value) => new(value);
}
