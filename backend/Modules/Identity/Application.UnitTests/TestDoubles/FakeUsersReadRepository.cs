using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application.Queries.CurrentUser;
using Conduit.Identity.Application.Queries.Profile;
using ErrorOr;

namespace Conduit.Identity.Application.UnitTests.TestDoubles;

internal sealed class FakeUsersReadRepository : IUsersReadRepository
{
    private readonly List<User> _users = [];
    private readonly HashSet<(string Follower, string Followed)> _follows = [];

    public void AddUser(User user) => _users.Add(user);

    public void AddFollow(string followerUsername, string followedUsername) =>
        _follows.Add((followerUsername, followedUsername));

    public Task<ErrorOr<User>> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        var user = _users.SingleOrDefault(u => u.Username == username);
        return Task.FromResult<ErrorOr<User>>(user is null ? Error.NotFound("User.NotFound") : user);
    }

    public Task<ErrorOr<Profile>> GetProfileAsync(string username, string? currentUsername, CancellationToken ct = default)
    {
        var user = _users.SingleOrDefault(u => u.Username == username);
        if (user is null)
        {
            return Task.FromResult<ErrorOr<Profile>>(Error.NotFound("User.NotFound"));
        }

        var following = currentUsername is not null && _follows.Contains((currentUsername, username));
        return Task.FromResult<ErrorOr<Profile>>(ToProfile(user, following));
    }

    public Task<IReadOnlyCollection<Profile>> GetProfilesAsync(IReadOnlyCollection<string> usernames, string? currentUsername, CancellationToken ct = default)
    {
        IReadOnlyCollection<Profile> profiles = [.. _users
            .Where(u => usernames.Contains(u.Username))
            .Select(u => ToProfile(u, currentUsername is not null && _follows.Contains((currentUsername, u.Username))))];
        return Task.FromResult(profiles);
    }

    public Task<IReadOnlyCollection<string>> GetFollowedUsernamesAsync(string followerUsername, CancellationToken ct = default)
    {
        IReadOnlyCollection<string> followed = [.. _follows
            .Where(f => f.Follower == followerUsername)
            .Select(f => f.Followed)];
        return Task.FromResult(followed);
    }

    private static Profile ToProfile(User user, bool following) => new()
    {
        Username = user.Username,
        Bio = user.Bio,
        Image = user.Image,
        Following = following,
    };
}
