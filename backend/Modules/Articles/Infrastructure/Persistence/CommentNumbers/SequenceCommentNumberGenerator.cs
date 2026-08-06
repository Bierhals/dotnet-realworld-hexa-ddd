using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application;
using Conduit.Articles.Domain;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Articles.Infrastructure.Persistence.CommentNumbers;

/// <summary>
/// Draws comment numbers from a real database sequence. Numbers a rolled-back request already
/// consumed are not returned to the sequence, so gaps are normal and harmless.
/// </summary>
internal sealed class SequenceCommentNumberGenerator(ArticlesDbContext dbContext) : ICommentNumberGenerator
{
    public async Task<CommentId> NextAsync(CancellationToken cancellationToken = default)
    {
        // The column has to be called "Value": that is the name EF Core expects when a raw query
        // returns a scalar.
        var sql = $"""
            SELECT nextval('"{ArticlesDbContext.SchemaName}"."{ArticlesDbContext.CommentNumbersName}"') AS "Value"
            """;

        var numbers = await dbContext.Database
            .SqlQueryRaw<long>(sql)
            .ToListAsync(cancellationToken);

        return CommentId.From((int)numbers[0]);
    }
}
