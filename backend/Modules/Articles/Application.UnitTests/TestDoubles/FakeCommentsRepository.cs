using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;

namespace Conduit.Articles.Application.UnitTests.TestDoubles;

internal sealed class FakeCommentsRepository : ICommentsRepository
{
    private readonly List<Comment> _comments = [];

    public IReadOnlyCollection<Comment> Comments => _comments;

    public void Seed(Comment comment)
    {
        comment.ClearDomainEvents();
        _comments.Add(comment);
    }

    public Task<Comment?> GetAsync(CommentId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_comments.Find(comment => comment.Id == id));

    public void Add(Comment comment) => _comments.Add(comment);

    public void Remove(Comment comment) => _comments.Remove(comment);

    public Task RemoveAllForArticleAsync(ArticleId articleId, CancellationToken cancellationToken = default)
    {
        _comments.RemoveAll(comment => comment.ArticleId == articleId);

        return Task.CompletedTask;
    }
}
