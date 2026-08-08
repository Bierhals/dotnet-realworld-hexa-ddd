using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;

namespace Conduit.Articles.Application.UnitTests.TestDoubles;

internal sealed class FakeArticleFavoritesRepository : IArticleFavoritesRepository
{
    private readonly List<ArticleFavorite> _favorites = [];

    public IReadOnlyCollection<ArticleFavorite> Favorites => _favorites;

    public int CountFor(ArticleId articleId) =>
        _favorites.FindAll(favorite => favorite.ArticleId == articleId).Count;

    public void Seed(ArticleId articleId, string username)
    {
        var favorite = ArticleFavorite.Create(articleId, Username.Create(username).Value);
        favorite.ClearDomainEvents();
        _favorites.Add(favorite);
    }

    public Task<ArticleFavorite?> GetAsync(
        ArticleId articleId,
        Username username,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_favorites.Find(favorite =>
            favorite.ArticleId == articleId && favorite.Username == username));

    public void Add(ArticleFavorite favorite) => _favorites.Add(favorite);

    public void Remove(ArticleFavorite favorite) => _favorites.Remove(favorite);

    public Task RemoveAllForArticleAsync(ArticleId articleId, CancellationToken cancellationToken = default)
    {
        _favorites.RemoveAll(favorite => favorite.ArticleId == articleId);

        return Task.CompletedTask;
    }
}
