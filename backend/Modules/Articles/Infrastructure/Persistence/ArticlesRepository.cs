using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Articles.Infrastructure.Persistence;

public sealed class ArticlesRepository(ArticlesDbContext dbContext) : IArticlesRepository
{
    public Task<Article?> GetBySlugAsync(ArticleSlug slug, CancellationToken cancellationToken = default) =>
        dbContext.Articles
            .Include(article => article.Comments)
            .Include("_tags")
            .Include("_favorites")
            .FirstOrDefaultAsync(article => article.Slug == slug, cancellationToken);

    public void Add(Article article) => dbContext.Articles.Add(article);

    public void Remove(Article article) => dbContext.Articles.Remove(article);
}
