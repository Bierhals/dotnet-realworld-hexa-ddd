using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Articles.Domain;

public interface ICommentsRepository
{
    public Task<Comment?> GetAsync(CommentId id, CancellationToken cancellationToken = default);

    public void Add(Comment comment);

    public void Remove(Comment comment);

    /// <summary>
    /// Drops every comment written on an article. A comment cannot outlive the article it was
    /// written on, and nothing in the database enforces that for us.
    /// </summary>
    public Task RemoveAllForArticleAsync(ArticleId articleId, CancellationToken cancellationToken = default);
}
