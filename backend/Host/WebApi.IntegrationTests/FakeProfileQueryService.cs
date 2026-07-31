using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Contracts.Queries;

namespace Conduit.Host.WebApi.IntegrationTests;

/// <summary>
/// In-memory stand-in for Identity's real IProfileQueryService, used by SliceFixture-based
/// tests that never wire up the real Identity module.
/// </summary>
public class FakeProfileQueryService : IProfileQueryService
{
    private record SeededProfile(string Username, string? Bio, string? Image);

    private readonly ConcurrentDictionary<string, SeededProfile> _profiles = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _follows = new();

    public void AddProfile(string username, string? bio = null, string? image = null) =>
        _profiles[username] = new SeededProfile(username, bio, image);

    public void AddFollow(string followerUsername, string followedUsername) =>
        _follows
            .GetOrAdd(followerUsername, _ => new HashSet<string>())
            .Add(followedUsername);

    public Task<IReadOnlyDictionary<string, ProfileDto>> GetProfilesAsync(
        IReadOnlyCollection<string> usernames,
        string? viewerUsername,
        CancellationToken ct = default
    )
    {
        var followedByViewer =
            viewerUsername is not null && _follows.TryGetValue(viewerUsername, out var followed)
                ? followed
                : new HashSet<string>();

        IReadOnlyDictionary<string, ProfileDto> result = usernames
            .Where(username => _profiles.ContainsKey(username))
            .Select(username => _profiles[username])
            .ToDictionary(
                p => p.Username,
                p => new ProfileDto(p.Username, p.Bio, p.Image, followedByViewer.Contains(p.Username))
            );

        return Task.FromResult(result);
    }

    public Task<IReadOnlyCollection<string>> GetFollowedUsernamesAsync(
        string followerUsername,
        CancellationToken ct = default
    )
    {
        IReadOnlyCollection<string> result = _follows.TryGetValue(followerUsername, out var followed)
            ? followed.ToList()
            : [];

        return Task.FromResult(result);
    }
}
