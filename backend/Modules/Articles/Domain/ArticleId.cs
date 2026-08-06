using System;

namespace Conduit.Articles.Domain;

public readonly record struct ArticleId
{
    public Guid Value { get; init; }

    private ArticleId(Guid value) => Value = value;

    public static ArticleId New() => new(Guid.CreateVersion7());

    public static ArticleId Rehydrate(string value) => new(Guid.Parse(value));
}
