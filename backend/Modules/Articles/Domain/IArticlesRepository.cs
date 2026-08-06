using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain.ValueObjects;

namespace Conduit.Articles.Domain;

public interface IArticlesRepository
{
    /// <summary>
    /// Loads an article with everything the aggregate needs to enforce its rules: its comments,
    /// its favorites and its tags.
    /// </summary>
    public Task<Article?> GetBySlugAsync(ArticleSlug slug, CancellationToken cancellationToken = default);

    public void Add(Article article);

    public void Remove(Article article);
}
