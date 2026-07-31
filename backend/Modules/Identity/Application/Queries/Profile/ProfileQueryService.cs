using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Contracts.Queries;

namespace Conduit.Identity.Application.Queries.Profile;

internal sealed class ProfileQueryService(IUsersReadRepository usersReadRepository) : IProfileQueryService
{
    public async Task<IReadOnlyDictionary<string, ProfileDto>> GetProfilesAsync(
        IReadOnlyCollection<string> usernames,
        string? viewerUsername,
        CancellationToken ct = default
    )
    {
        var profiles = await usersReadRepository.GetProfilesAsync(usernames, viewerUsername, ct);
        return profiles.ToDictionary(
            p => p.Username,
            p => new ProfileDto(p.Username, p.Bio, p.Image, p.Following)
        );
    }

    public Task<IReadOnlyCollection<string>> GetFollowedUsernamesAsync(
        string followerUsername,
        CancellationToken ct = default
    ) => usersReadRepository.GetFollowedUsernamesAsync(followerUsername, ct);
}
