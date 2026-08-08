using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Articles.Application;

/// <summary>
/// Deliberately exposes saving only, without transaction control: the article use cases also talk
/// to the Tags module, which writes through its own DbContext and connection. Holding an open
/// write transaction across a handler would block that second connection - on SQLite, where both
/// contexts share one file, the request then fails with "database is locked".
/// </summary>
public interface IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken);
}
