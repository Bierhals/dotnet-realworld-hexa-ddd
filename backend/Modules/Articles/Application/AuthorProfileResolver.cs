using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Articles.Application;

/// <summary>
/// Turns projections into read models by resolving their authors in a single batched lookup. An
/// author whose profile cannot be found falls back to a bare profile carrying just the username,
/// so a deleted account never makes an article unreadable.
/// </summary>
internal static class AuthorProfileResolver
{
    public static async Task<IReadOnlyCollection<ArticleReadModel>> ToReadModelsAsync(
        IReadOnlyCollection<ArticleProjection> articles,
        IProfileReader profileReader,
        string? viewerUsername,
        CancellationToken cancellationToken)
    {
        var profiles = await GetProfilesAsync(
            [.. articles.Select(article => article.AuthorUsername).Distinct()],
            profileReader,
            viewerUsername,
            cancellationToken);

        return [.. articles.Select(article => new ArticleReadModel(
            article.Slug,
            article.Title,
            article.Description,
            article.Body,
            article.TagList,
            article.CreatedAt,
            article.UpdatedAt,
            article.Favorited,
            article.FavoritesCount,
            Resolve(profiles, article.AuthorUsername)))];
    }

    public static async Task<IReadOnlyCollection<CommentReadModel>> ToReadModelsAsync(
        IReadOnlyCollection<CommentProjection> comments,
        IProfileReader profileReader,
        string? viewerUsername,
        CancellationToken cancellationToken)
    {
        var profiles = await GetProfilesAsync(
            [.. comments.Select(comment => comment.AuthorUsername).Distinct()],
            profileReader,
            viewerUsername,
            cancellationToken);

        return [.. comments.Select(comment => new CommentReadModel(
            comment.Id,
            comment.Body,
            comment.CreatedAt,
            comment.UpdatedAt,
            Resolve(profiles, comment.AuthorUsername)))];
    }

    private static async Task<IReadOnlyDictionary<string, AuthorProfile>> GetProfilesAsync(
        IReadOnlyCollection<string> usernames,
        IProfileReader profileReader,
        string? viewerUsername,
        CancellationToken cancellationToken) =>
        usernames.Count == 0
            ? new Dictionary<string, AuthorProfile>()
            : await profileReader.GetAuthorProfilesAsync(usernames, viewerUsername, cancellationToken);

    private static AuthorProfile Resolve(IReadOnlyDictionary<string, AuthorProfile> profiles, string username) =>
        profiles.TryGetValue(username, out var profile)
            ? profile
            : new AuthorProfile(username, null, null, false);
}
