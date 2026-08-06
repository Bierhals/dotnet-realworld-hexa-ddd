using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Articles.Application;

/// <summary>
/// The read side of the module: projected, already-paged data straight from storage. It never
/// returns domain objects, and it does not know anything about author profiles - those are owned
/// by another module and are added by the query handlers through <see cref="IProfileReader"/>.
/// </summary>
public interface IArticlesReadRepository
{
    public Task<ArticleProjection?> GetBySlugAsync(
        string slug,
        string? viewerUsername,
        CancellationToken cancellationToken = default);

    public Task<ArticleProjectionPage> ListAsync(
        ArticleListFilter filter,
        string? viewerUsername,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>null</c> when the article itself does not exist, which the caller has to report
    /// differently from an article that simply has no comments yet.
    /// </summary>
    public Task<IReadOnlyCollection<CommentProjection>?> GetCommentsAsync(
        string slug,
        CancellationToken cancellationToken = default);
}

public sealed record ArticleProjection(
    string Slug,
    string Title,
    string Description,
    string Body,
    IReadOnlyCollection<string> TagList,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool Favorited,
    int FavoritesCount,
    string AuthorUsername);

public sealed record ArticleProjectionPage(IReadOnlyCollection<ArticleProjection> Articles, int TotalCount);

public sealed record CommentProjection(
    int Id,
    string Body,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string AuthorUsername);

/// <summary>
/// All filters are optional; <see cref="Authors"/> restricts the result to a set of authors and is
/// what turns a listing into a personal feed.
/// </summary>
public sealed record ArticleListFilter
{
    public string? Tag { get; init; }

    public string? Author { get; init; }

    public string? FavoritedBy { get; init; }

    public IReadOnlyCollection<string>? Authors { get; init; }

    public required int Limit { get; init; }

    public required int Offset { get; init; }
}
