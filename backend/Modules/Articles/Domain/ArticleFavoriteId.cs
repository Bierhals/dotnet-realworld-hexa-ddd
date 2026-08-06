using System;

namespace Conduit.Articles.Domain;

public readonly record struct ArticleFavoriteId
{
    public Guid Value { get; init; }

    private ArticleFavoriteId(Guid value) => Value = value;

    public static ArticleFavoriteId New() => new(Guid.CreateVersion7());

    public static ArticleFavoriteId Rehydrate(string value) => new(Guid.Parse(value));
}
