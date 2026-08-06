using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application;
using Conduit.Articles.Domain;

namespace Conduit.Articles.Application.UnitTests.TestDoubles;

/// <summary>
/// Stands in for the database sequence, handing out 1, 2, 3, ... in order.
/// </summary>
internal sealed class FakeCommentNumberGenerator : ICommentNumberGenerator
{
    private int _next;

    public Task<CommentId> NextAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(CommentId.From(++_next));
}
