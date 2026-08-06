using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Conduit.Articles.Api.Endpoints.Articles;

public sealed record ArticleResponse
{
    public required string Slug { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string Body { get; init; }

    public required IReadOnlyCollection<string> TagList { get; init; }

    public required DateTime CreatedAt { get; init; }

    public required DateTime UpdatedAt { get; init; }

    public required bool Favorited { get; init; }

    public required int FavoritesCount { get; init; }

    public required AuthorResponse Author { get; init; }
}

public sealed record AuthorResponse
{
    public required string Username { get; init; }

    // The RealWorld spec requires bio/image to always be present in the response (as null when
    // unset), so the global JsonIgnoreCondition.WhenWritingNull must not drop these two.
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? Bio { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? Image { get; init; }

    public required bool Following { get; init; }
}

public sealed record ArticleEnvelope(ArticleResponse Article);

public sealed record ArticlesEnvelope(IReadOnlyCollection<ArticleResponse> Articles, int ArticlesCount);
