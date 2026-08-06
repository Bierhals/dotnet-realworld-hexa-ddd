using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain.ValueObjects;

namespace Conduit.Articles.Domain;

public interface IArticleFavoritesRepository
{
    public Task<ArticleFavorite?> GetAsync(
        ArticleId articleId,
        AuthorUsername username,
        CancellationToken cancellationToken = default);

    public void Add(ArticleFavorite favorite);

    public void Remove(ArticleFavorite favorite);

    /// <summary>
    /// Drops every favorite of an article. A favorite cannot outlive the article it points at,
    /// and nothing in the database enforces that for us.
    /// </summary>
    public Task RemoveAllForArticleAsync(ArticleId articleId, CancellationToken cancellationToken = default);
}
