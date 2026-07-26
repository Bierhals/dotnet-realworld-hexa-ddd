using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application.Queries.CurrentUser;
using Conduit.Identity.Application.Queries.Profile;
using ErrorOr;

namespace Conduit.Identity.Application;

public interface IUsersReadRepository
{
    public Task<ErrorOr<User>> GetByUsernameAsync(string username, CancellationToken ct = default);

    public Task<ErrorOr<Profile>> GetProfileAsync(string username, string? currentUsername, CancellationToken ct = default);
}
