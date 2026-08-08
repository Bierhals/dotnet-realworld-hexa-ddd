using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Articles.Infrastructure.Persistence;

public sealed class ArticlesReadRepository(ArticlesDbContext dbContext) : IArticlesReadRepository
{
    public async Task<ArticleProjection?> GetBySlugAsync(
        string slug,
        string? viewerUsername,
        CancellationToken cancellationToken = default)
    {
        var wantedSlug = ArticleSlug.Rehydrate(slug);

        var row = await Project(
                dbContext.Articles.AsNoTracking().Where(article => article.Slug == wantedSlug),
                viewerUsername)
            .FirstOrDefaultAsync(cancellationToken);

        return row is null ? null : ToProjection(row);
    }

    public async Task<ArticleProjectionPage> ListAsync(
        ArticleListFilter filter,
        string? viewerUsername,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Articles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Tag))
        {
            var tagName = filter.Tag;
            query = query.Where(article => article.Tags.Any(tag => tag.Value == tagName));
        }

        if (!string.IsNullOrWhiteSpace(filter.Author))
        {
            var author = Username.Rehydrate(filter.Author);
            query = query.Where(article => article.Author == author);
        }

        if (!string.IsNullOrWhiteSpace(filter.FavoritedBy))
        {
            var favoritedBy = Username.Rehydrate(filter.FavoritedBy);
            query = query.Where(article => dbContext.ArticleFavorites
                .Any(favorite => favorite.ArticleId == article.Id && favorite.Username == favoritedBy));
        }

        if (filter.Authors is not null)
        {
            var authors = filter.Authors.Select(Username.Rehydrate).ToList();
            query = query.Where(article => authors.Contains(article.Author));
        }

        // Counted before paging, so the caller learns how many articles match the filter overall.
        var totalCount = await query.CountAsync(cancellationToken);

        // Ordering and paging happen on the articles themselves; projecting first would leave the
        // database sorting by a shape it cannot translate.
        var page = query
            .OrderByDescending(article => article.CreatedAtUtc)
            .Skip(filter.Offset)
            .Take(filter.Limit);

        var rows = await Project(page, viewerUsername).ToListAsync(cancellationToken);

        return new ArticleProjectionPage(rows.ConvertAll(ToProjection), totalCount);
    }

    public async Task<IReadOnlyCollection<CommentProjection>?> GetCommentsAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var wantedSlug = ArticleSlug.Rehydrate(slug);

        var articleId = await dbContext.Articles.AsNoTracking()
            .Where(article => article.Slug == wantedSlug)
            .Select(article => (ArticleId?)article.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (articleId is null)
        {
            return null;
        }

        var rows = await dbContext.Comments.AsNoTracking()
            .Where(comment => comment.ArticleId == articleId.Value)
            .OrderBy(comment => comment.Id)
            .Select(comment => new CommentRow(
                comment.Id,
                comment.Body,
                comment.CreatedAtUtc,
                comment.UpdatedAtUtc,
                comment.Author))
            .ToListAsync(cancellationToken);

        return rows.ConvertAll(row => new CommentProjection(
            row.Id.Value,
            row.Body.Value,
            row.CreatedAt,
            row.UpdatedAt,
            row.Author.Value));
    }

    /// <summary>
    /// Projects to value objects rather than to their raw values: the columns are value-converted,
    /// so unwrapping them has to happen after materialization, not inside the query.
    /// </summary>
    private IQueryable<ArticleRow> Project(IQueryable<Article> articles, string? viewerUsername)
    {
        var viewer = viewerUsername is null ? null : Username.Rehydrate(viewerUsername);

        return articles.Select(article => new ArticleRow(
            article.Slug,
            article.Title,
            article.Description,
            article.Body,
            article.Tags.Select(tag => tag.Value).ToList(),
            article.CreatedAtUtc,
            article.UpdatedAtUtc,
            viewer != null && dbContext.ArticleFavorites
                .Any(favorite => favorite.ArticleId == article.Id && favorite.Username == viewer),
            dbContext.ArticleFavorites.Count(favorite => favorite.ArticleId == article.Id),
            article.Author));
    }

    private static ArticleProjection ToProjection(ArticleRow row) => new(
        row.Slug.Value,
        row.Title.Value,
        row.Description.Value,
        row.Body.Value,
        row.TagList,
        row.CreatedAt,
        row.UpdatedAt,
        row.Favorited,
        row.FavoritesCount,
        row.Author.Value);

    private sealed record ArticleRow(
        ArticleSlug Slug,
        ArticleTitle Title,
        ArticleDescription Description,
        ArticleBody Body,
        List<string> TagList,
        System.DateTime CreatedAt,
        System.DateTime UpdatedAt,
        bool Favorited,
        int FavoritesCount,
        Username Author);

    private sealed record CommentRow(
        CommentId Id,
        CommentBody Body,
        System.DateTime CreatedAt,
        System.DateTime UpdatedAt,
        Username Author);
}
