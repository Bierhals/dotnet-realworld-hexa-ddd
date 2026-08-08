using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Articles.Infrastructure.Persistence;

public sealed class ArticlesRepository(ArticlesDbContext dbContext) : IArticlesRepository
{
    // The tags come along automatically - they are owned by the article, not a separate table the
    // repository has to remember to include.
    public Task<Article?> GetBySlugAsync(ArticleSlug slug, CancellationToken cancellationToken = default) =>
        dbContext.Articles.FirstOrDefaultAsync(article => article.Slug == slug, cancellationToken);

    public async Task<ArticleId?> GetIdBySlugAsync(ArticleSlug slug, CancellationToken cancellationToken = default) =>
        await dbContext.Articles
            .Where(article => article.Slug == slug)
            .Select(article => (ArticleId?)article.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public void Add(Article article) => dbContext.Articles.Add(article);

    public void Remove(Article article) => dbContext.Articles.Remove(article);
}
