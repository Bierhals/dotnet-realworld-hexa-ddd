using Conduit.Articles.Domain.Rules;
using ErrorOr;

namespace Conduit.Articles.Domain.ValueObjects;

public sealed record CommentBody
{
    public string Value { get; }

    private CommentBody(string value) => Value = value;

    public static ErrorOr<CommentBody> Create(string value)
    {
        var sanitizedValue = value ?? string.Empty;

        var check = new CommentBodyIsNotEmpty(sanitizedValue).Check();

        return check.IsError ? check.Errors : new CommentBody(sanitizedValue);
    }

    public static CommentBody Rehydrate(string value) => new(value);
}
