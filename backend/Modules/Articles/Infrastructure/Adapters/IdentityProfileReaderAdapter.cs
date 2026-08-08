using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application;
using Conduit.Identity.Contracts.Queries;

namespace Conduit.Articles.Infrastructure.Adapters;

/// <summary>
/// Adapts the Identity module's public query contract to the port the Articles module owns. This
/// is the only place in Articles that is allowed to know Identity exists.
/// </summary>
internal sealed class IdentityProfileReaderAdapter(IProfileQueryService profileQueryService) : IProfileReader
{
    public async Task<IReadOnlyDictionary<string, AuthorProfile>> GetAuthorProfilesAsync(
        IReadOnlyCollection<string> usernames,
        string? viewerUsername,
        CancellationToken cancellationToken = default)
    {
        var profiles = await profileQueryService.GetProfilesAsync(usernames, viewerUsername, cancellationToken);

        return profiles.ToDictionary(
            profile => profile.Key,
            profile => new AuthorProfile(
                profile.Value.Username,
                profile.Value.Bio,
                profile.Value.Image,
                profile.Value.Following));
    }

    public Task<IReadOnlyCollection<string>> GetFollowedAuthorsAsync(
        string followerUsername,
        CancellationToken cancellationToken = default) =>
        profileQueryService.GetFollowedUsernamesAsync(followerUsername, cancellationToken);
}
