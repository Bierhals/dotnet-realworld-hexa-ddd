using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Host.WebApi.Domain;
using Conduit.Identity.Contracts.Queries;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Host.WebApi.Features.Articles;

public static class ArticleExtensions
{
    public static IQueryable<Article> GetAllData(this DbSet<Article> articles) =>
        articles.Include(x => x.ArticleFavorites).Include(x => x.ArticleTags).AsNoTracking();

    public static async Task EnrichAuthorsAsync(
        this IEnumerable<Article> articles,
        IProfileQueryService profileQueryService,
        string? viewerUsername,
        CancellationToken cancellationToken
    )
    {
        var articleList = articles as IReadOnlyCollection<Article> ?? [.. articles];
        var usernames = articleList.Select(x => x.AuthorUsername).Distinct().ToList();
        var profiles = await profileQueryService.GetProfilesAsync(
            usernames,
            viewerUsername,
            cancellationToken
        );

        foreach (var article in articleList)
        {
            article.Author = profiles.TryGetValue(article.AuthorUsername, out var profile)
                ? new AuthorProfile
                {
                    Username = profile.Username,
                    Bio = profile.Bio,
                    Image = profile.Image,
                    Following = profile.Following,
                }
                : new AuthorProfile { Username = article.AuthorUsername };
        }
    }
}
