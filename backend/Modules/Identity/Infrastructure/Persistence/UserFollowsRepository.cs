using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Identity.Infrastructure.Persistence;

public sealed class UserFollowsRepository(IdentityDbContext dbContext) : IUserFollowsRepository
{
    public Task<bool> ExistsAsync(UserId followedUserId, UserId followerUserId, CancellationToken ct = default) =>
        dbContext.UserFollows.AnyAsync(f => f.FollowedUserId == followedUserId && f.FollowerUserId == followerUserId, ct);

    public Task<UserFollow?> GetAsync(UserId followedUserId, UserId followerUserId, CancellationToken ct = default) =>
        dbContext.UserFollows.FirstOrDefaultAsync(f => f.FollowedUserId == followedUserId && f.FollowerUserId == followerUserId, ct);

    public async Task AddAsync(UserFollow userFollow, CancellationToken ct = default) =>
        await dbContext.UserFollows.AddAsync(userFollow, ct);

    public Task RemoveAsync(UserFollow userFollow, CancellationToken ct = default)
    {
        dbContext.UserFollows.Remove(userFollow);
        return Task.CompletedTask;
    }
}
