using System.Threading;

namespace Conduit.Domain.User;

public interface IUsersCounter
{
    uint CountUsersWithEmail(string email, CancellationToken cancellationToken);
    uint CountUsersWithUsername(string username, CancellationToken cancellationToken);
}
