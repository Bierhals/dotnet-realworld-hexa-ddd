using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Articles.Domain;

/// <summary>
/// Hands out the next comment number from a database sequence. Keeping this behind a port lets the
/// aggregate stay free of I/O: the use case fetches the number and passes it in.
/// </summary>
public interface ICommentNumberGenerator
{
    public Task<CommentId> NextAsync(CancellationToken cancellationToken = default);
}
