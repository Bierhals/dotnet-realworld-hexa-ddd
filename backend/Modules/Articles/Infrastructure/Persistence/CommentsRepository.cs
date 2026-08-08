using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Articles.Infrastructure.Persistence;

public sealed class CommentsRepository(ArticlesDbContext dbContext) : ICommentsRepository
{
    public Task<Comment?> GetAsync(CommentId id, CancellationToken cancellationToken = default) =>
        dbContext.Comments.FirstOrDefaultAsync(comment => comment.Id == id, cancellationToken);

    public void Add(Comment comment) => dbContext.Comments.Add(comment);

    public void Remove(Comment comment) => dbContext.Comments.Remove(comment);

    public async Task RemoveAllForArticleAsync(ArticleId articleId, CancellationToken cancellationToken = default)
    {
        var comments = await dbContext.Comments
            .Where(comment => comment.ArticleId == articleId)
            .ToListAsync(cancellationToken);

        dbContext.Comments.RemoveRange(comments);
    }
}
