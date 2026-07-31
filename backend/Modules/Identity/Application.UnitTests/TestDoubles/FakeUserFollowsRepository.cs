using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain;

namespace Conduit.Identity.Application.UnitTests.TestDoubles;

internal sealed class FakeUserFollowsRepository : IUserFollowsRepository
{
    private readonly List<UserFollow> _userFollows = [];

    public IReadOnlyList<UserFollow> UserFollows => _userFollows;

    public Task<bool> ExistsAsync(UserId followedUserId, UserId followerUserId, CancellationToken ct = default) =>
        Task.FromResult(_userFollows.Any(f => f.FollowedUserId == followedUserId && f.FollowerUserId == followerUserId));

    public Task<UserFollow?> GetAsync(UserId followedUserId, UserId followerUserId, CancellationToken ct = default)
    {
        var userFollow = _userFollows.SingleOrDefault(f => f.FollowedUserId == followedUserId && f.FollowerUserId == followerUserId);
        return Task.FromResult(userFollow);
    }

    public Task AddAsync(UserFollow userFollow, CancellationToken ct = default)
    {
        _userFollows.Add(userFollow);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(UserFollow userFollow, CancellationToken ct = default)
    {
        _userFollows.Remove(userFollow);
        return Task.CompletedTask;
    }
}
