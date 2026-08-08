using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Articles.Infrastructure.Persistence.CommentNumbers;

/// <summary>
/// Stands in for <see cref="SequenceCommentNumberGenerator"/> on providers without sequences.
/// The single counter row is advanced and read back in one statement, so two concurrent requests
/// cannot be handed the same number.
/// </summary>
internal sealed class CounterTableCommentNumberGenerator(ArticlesDbContext dbContext) : ICommentNumberGenerator
{
    public async Task<CommentId> NextAsync(CancellationToken cancellationToken = default)
    {
        // The column has to be called "Value": that is the name EF Core expects when a raw query
        // returns a scalar. The result is materialized with ToListAsync rather than SingleAsync
        // because EF Core cannot compose a LIMIT onto an UPDATE statement.
        var sql = $"""
            UPDATE "{ArticlesDbContext.CommentNumbersName}"
            SET "NextValue" = "NextValue" + 1
            WHERE "Id" = {CommentNumberCounter.SingletonId}
            RETURNING "NextValue" AS "Value"
            """;

        var numbers = await dbContext.Database
            .SqlQueryRaw<int>(sql)
            .ToListAsync(cancellationToken);

        return CommentId.From(numbers[0]);
    }
}
