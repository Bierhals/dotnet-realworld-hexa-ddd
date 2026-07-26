using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application.Queries.CurrentUser;
using ErrorOr;

namespace Conduit.Identity.Application;

public interface IUsersReadRepository
{
    public Task<ErrorOr<User>> GetByUsernameAsync(string username, CancellationToken ct = default);
}
