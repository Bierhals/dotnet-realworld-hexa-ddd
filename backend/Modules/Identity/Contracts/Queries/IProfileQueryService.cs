using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Identity.Contracts.Queries;

public interface IProfileQueryService
{
    public Task<IReadOnlyDictionary<string, ProfileDto>> GetProfilesAsync(
        IReadOnlyCollection<string> usernames,
        string? viewerUsername,
        CancellationToken ct = default
    );

    public Task<IReadOnlyCollection<string>> GetFollowedUsernamesAsync(
        string followerUsername,
        CancellationToken ct = default
    );
}

public sealed record ProfileDto(string Username, string? Bio, string? Image, bool Following);
