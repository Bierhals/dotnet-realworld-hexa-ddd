using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain.ValueObjects;

namespace Conduit.Articles.Domain;

public interface IArticlesRepository
{
    public Task<Article?> GetBySlugAsync(ArticleSlug slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the public slug to the identity that comments and favorites refer to, without
    /// loading the article itself.
    /// </summary>
    public Task<ArticleId?> GetIdBySlugAsync(ArticleSlug slug, CancellationToken cancellationToken = default);

    public void Add(Article article);

    public void Remove(Article article);
}
