using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Domain.User;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task<User> GetByIdAsync(UserEmail id, CancellationToken cancellationToken);
}
