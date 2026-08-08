using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Articles.Application;

/// <summary>
/// Author display data, which the Articles module does not own. The port and its DTO belong to
/// this module; the adapter that fetches the data from whoever owns accounts lives in
/// Conduit.Articles.Infrastructure.
/// </summary>
public interface IProfileReader
{
    /// <summary>
    /// Batched on purpose - enriching a list of articles must not turn into one lookup per author.
    /// Usernames without a profile are simply absent from the result.
    /// </summary>
    public Task<IReadOnlyDictionary<string, AuthorProfile>> GetAuthorProfilesAsync(
        IReadOnlyCollection<string> usernames,
        string? viewerUsername,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyCollection<string>> GetFollowedAuthorsAsync(
        string followerUsername,
        CancellationToken cancellationToken = default);
}

public sealed record AuthorProfile(string Username, string? Bio, string? Image, bool Following);
