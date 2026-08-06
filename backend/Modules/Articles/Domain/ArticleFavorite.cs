using Conduit.Articles.Domain.ValueObjects;

namespace Conduit.Articles.Domain;

/// <summary>
/// Records that an account has favorited an article. Part of the <see cref="Article"/> aggregate.
/// </summary>
public sealed class ArticleFavorite
{
    public ArticleId ArticleId { get; private set; }
    public AuthorUsername Username { get; private set; }

#pragma warning disable CS8618 // Non-nullable properties are populated by EF Core when materializing.
    private ArticleFavorite() { } // for EF Core
#pragma warning restore CS8618

    internal ArticleFavorite(ArticleId articleId, AuthorUsername username)
    {
        ArticleId = articleId;
        Username = username;
    }
}
