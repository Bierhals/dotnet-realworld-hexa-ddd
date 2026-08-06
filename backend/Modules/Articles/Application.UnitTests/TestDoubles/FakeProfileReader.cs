using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application;

namespace Conduit.Articles.Application.UnitTests.TestDoubles;

internal sealed class FakeProfileReader : IProfileReader
{
    private readonly Dictionary<string, AuthorProfile> _profiles = [];
    private readonly Dictionary<string, List<string>> _following = [];

    public void Seed(string username, string? bio = null, string? image = null) =>
        _profiles[username] = new AuthorProfile(username, bio, image, false);

    public void Follows(string follower, params string[] followed) =>
        _following[follower] = [.. followed];

    public Task<IReadOnlyDictionary<string, AuthorProfile>> GetAuthorProfilesAsync(
        IReadOnlyCollection<string> usernames,
        string? viewerUsername,
        CancellationToken cancellationToken = default)
    {
        var followedByViewer = viewerUsername is not null && _following.TryGetValue(viewerUsername, out var followed)
            ? followed
            : [];

        IReadOnlyDictionary<string, AuthorProfile> result = usernames
            .Where(_profiles.ContainsKey)
            .ToDictionary(
                username => username,
                username => _profiles[username] with { Following = followedByViewer.Contains(username) });

        return Task.FromResult(result);
    }

    public Task<IReadOnlyCollection<string>> GetFollowedAuthorsAsync(
        string followerUsername,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<string> followed = _following.TryGetValue(followerUsername, out var names) ? names : [];

        return Task.FromResult(followed);
    }
}
