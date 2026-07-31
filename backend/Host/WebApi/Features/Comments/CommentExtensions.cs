using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Host.WebApi.Domain;
using Conduit.Identity.Contracts.Queries;

namespace Conduit.Host.WebApi.Features.Comments;

public static class CommentExtensions
{
    public static async Task EnrichAuthorsAsync(
        this IEnumerable<Comment> comments,
        IProfileQueryService profileQueryService,
        string? viewerUsername,
        CancellationToken cancellationToken
    )
    {
        var commentList = comments as IReadOnlyCollection<Comment> ?? [.. comments];
        var usernames = commentList.Select(x => x.AuthorUsername).Distinct().ToList();
        var profiles = await profileQueryService.GetProfilesAsync(
            usernames,
            viewerUsername,
            cancellationToken
        );

        foreach (var comment in commentList)
        {
            comment.Author = profiles.TryGetValue(comment.AuthorUsername, out var profile)
                ? new AuthorProfile
                {
                    Username = profile.Username,
                    Bio = profile.Bio,
                    Image = profile.Image,
                    Following = profile.Following,
                }
                : new AuthorProfile { Username = comment.AuthorUsername };
        }
    }
}
