using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application;

namespace Conduit.Articles.Application.UnitTests.TestDoubles;

internal sealed class FakeArticlesReadRepository : IArticlesReadRepository
{
    private static readonly DateTime PublishedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly List<StoredArticle> _articles = [];
    private readonly Dictionary<string, List<CommentProjection>> _comments = [];

    public ArticleListFilter? LastFilter { get; private set; }

    public void Seed(string slug, string author, IReadOnlyCollection<string>? tags = null, IReadOnlyCollection<string>? favoritedBy = null)
    {
        _articles.Add(new StoredArticle(slug, author, tags ?? [], favoritedBy ?? []));
        _comments[slug] = [];
    }

    public void SeedComment(string slug, int id, string author, string body = "nice") =>
        _comments[slug].Add(new CommentProjection(id, body, PublishedAt, PublishedAt, author));

    public Task<ArticleProjection?> GetBySlugAsync(string slug, string? viewerUsername, CancellationToken cancellationToken = default)
    {
        var stored = _articles.Find(article => article.Slug == slug);

        return Task.FromResult(stored is null ? null : ToProjection(stored, viewerUsername));
    }

    public Task<ArticleProjectionPage> ListAsync(ArticleListFilter filter, string? viewerUsername, CancellationToken cancellationToken = default)
    {
        LastFilter = filter;

        var matching = _articles.Where(article =>
            (filter.Tag is null || article.Tags.Contains(filter.Tag))
            && (filter.Author is null || article.Author == filter.Author)
            && (filter.FavoritedBy is null || article.FavoritedBy.Contains(filter.FavoritedBy))
            && (filter.Authors is null || filter.Authors.Contains(article.Author)))
            .ToList();

        var page = matching.Skip(filter.Offset).Take(filter.Limit)
            .Select(article => ToProjection(article, viewerUsername))
            .ToList();

        return Task.FromResult(new ArticleProjectionPage(page, matching.Count));
    }

    public Task<IReadOnlyCollection<CommentProjection>?> GetCommentsAsync(string slug, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<CommentProjection>? comments = _comments.TryGetValue(slug, out var found) ? found : null;

        return Task.FromResult(comments);
    }

    private static ArticleProjection ToProjection(StoredArticle article, string? viewerUsername) => new(
        article.Slug,
        "How to train your dragon",
        "Ever wonder how?",
        "You have to believe",
        article.Tags,
        PublishedAt,
        PublishedAt,
        viewerUsername is not null && article.FavoritedBy.Contains(viewerUsername),
        article.FavoritedBy.Count,
        article.Author);

    private sealed record StoredArticle(
        string Slug,
        string Author,
        IReadOnlyCollection<string> Tags,
        IReadOnlyCollection<string> FavoritedBy);
}
