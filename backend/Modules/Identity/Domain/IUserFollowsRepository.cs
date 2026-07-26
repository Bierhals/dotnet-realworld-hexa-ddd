using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Identity.Domain;

public interface IUserFollowsRepository
{
    public Task<bool> ExistsAsync(UserId followedUserId, UserId followerUserId, CancellationToken ct = default);
    public Task AddAsync(UserFollow userFollow, CancellationToken ct = default);
}
