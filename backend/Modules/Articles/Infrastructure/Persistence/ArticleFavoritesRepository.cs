using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Articles.Infrastructure.Persistence;

public sealed class ArticleFavoritesRepository(ArticlesDbContext dbContext) : IArticleFavoritesRepository
{
    public Task<ArticleFavorite?> GetAsync(
        ArticleId articleId,
        AuthorUsername username,
        CancellationToken cancellationToken = default) =>
        dbContext.ArticleFavorites.FirstOrDefaultAsync(
            favorite => favorite.ArticleId == articleId && favorite.Username == username,
            cancellationToken);

    public void Add(ArticleFavorite favorite) => dbContext.ArticleFavorites.Add(favorite);

    public void Remove(ArticleFavorite favorite) => dbContext.ArticleFavorites.Remove(favorite);

    public async Task RemoveAllForArticleAsync(ArticleId articleId, CancellationToken cancellationToken = default)
    {
        var favorites = await dbContext.ArticleFavorites
            .Where(favorite => favorite.ArticleId == articleId)
            .ToListAsync(cancellationToken);

        dbContext.ArticleFavorites.RemoveRange(favorites);
    }
}
