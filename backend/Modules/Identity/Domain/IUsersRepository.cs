using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain.ValueObjects;
using ErrorOr;

namespace Conduit.Identity.Domain;

public interface IUsersRepository
{
    public Task<ErrorOr<User>> GetByEmailAsync(UserEmail email, CancellationToken ct = default);
    public Task<ErrorOr<User>> GetByUsernameAsync(Username username, CancellationToken ct = default);
    public Task AddAsync(User user, CancellationToken ct = default);
    public Task<bool> ExistsByEmailAsync(UserEmail email, CancellationToken ct = default);
    public Task<bool> ExistsByEmailAsync(UserEmail email, UserId excludingUserId, CancellationToken ct = default);
    public Task<bool> ExistsByUsernameAsync(Username username, CancellationToken ct = default);
    public Task<bool> ExistsByUsernameAsync(Username username, UserId excludingUserId, CancellationToken ct = default);
}
