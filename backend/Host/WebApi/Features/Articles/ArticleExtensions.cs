using System.Linq;
using Conduit.Host.WebApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Host.WebApi.Features.Articles;

public static class ArticleExtensions
{
    public static IQueryable<Article> GetAllData(this DbSet<Article> articles) =>
        articles
            .Include(x => x.Author)
            .Include(x => x.ArticleFavorites)
            .Include(x => x.ArticleTags)
            .AsNoTracking();
}
