using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Users.Domain.Article;

public interface IArticleCounter
{
    Task<int> CountArticlesWithSlugAsync(ArticleSlug slug, CancellationToken cancellationToken = default);
    Task<int> CountArticlesWithTitleAsync(ArticleTitle title, CancellationToken cancellationToken = default);
}
