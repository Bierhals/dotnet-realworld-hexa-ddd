using Conduit.Articles.Domain.Events;
using Conduit.Articles.Domain.ValueObjects;
using Conduit.Shared.Domain;

namespace Conduit.Articles.Domain;

/// <summary>
/// Records that an account has favorited an article. Its own aggregate, like every link between
/// two things that each outlive it - both sides are referred to by identity only.
/// </summary>
public sealed class ArticleFavorite : AggregateRoot<ArticleFavoriteId>
{
    public ArticleId ArticleId { get; private set; }
    public AuthorUsername Username { get; private set; }

#pragma warning disable CS8618 // Non-nullable properties are populated by EF Core when materializing.
    private ArticleFavorite() { } // for EF Core
#pragma warning restore CS8618

    private ArticleFavorite(ArticleFavoriteId id, ArticleId articleId, AuthorUsername username) : base(id)
    {
        ArticleId = articleId;
        Username = username;
    }

    public static ArticleFavorite Create(ArticleId articleId, AuthorUsername username)
    {
        var favorite = new ArticleFavorite(ArticleFavoriteId.New(), articleId, username);
        favorite.AddDomainEvent(new ArticleFavoritedDomainEvent(articleId.Value, username.Value));

        return favorite;
    }

    /// <summary>
    /// Announces that the article is no longer favorited. Called right before the repository
    /// removes this record.
    /// </summary>
    public void Remove() => AddDomainEvent(new ArticleUnfavoritedDomainEvent(ArticleId.Value, Username.Value));
}
