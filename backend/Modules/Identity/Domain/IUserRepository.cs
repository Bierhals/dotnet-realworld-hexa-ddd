using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain.ValueObjects;

namespace Conduit.Identity.Domain;

public interface IUserRepository
{
    public Task<bool> ExistsByEmailAsync(UserEmail email, CancellationToken ct = default);
    public Task<bool> ExistsByEmailAsync(UserEmail email, UserId excludingUserId, CancellationToken ct = default);
    public Task<bool> ExistsByUsernameAsync(Username username, CancellationToken ct = default);
    public Task<bool> ExistsByUsernameAsync(Username username, UserId excludingUserId, CancellationToken ct = default);
}