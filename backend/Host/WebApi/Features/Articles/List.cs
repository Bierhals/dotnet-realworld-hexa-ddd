using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Host.WebApi.Infrastructure;
using Conduit.Host.WebApi.Shared.RequestHandling;
using Conduit.Identity.Contracts.Queries;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Host.WebApi.Features.Articles;

public class List
{
    public record Query(
        string Tag,
        string Author,
        string FavoritedUsername,
        int? Limit,
        int? Offset,
        bool IsFeed = false
    ) : IQuery<ArticlesEnvelope>;

    public class Handler(
        ConduitContext context,
        ICurrentUserAccessor currentUserAccessor,
        IProfileQueryService profileQueryService
    ) : IQueryHandler<Query, ArticlesEnvelope>
    {
        public async Task<ArticlesEnvelope> Handle(
            Query message,
            CancellationToken cancellationToken
        )
        {
            var currentUsername = currentUserAccessor.GetCurrentUsername();
            var queryable = context.Articles.GetAllData();

            if (message.IsFeed && currentUsername != null)
            {
                var followedUsernames = await profileQueryService.GetFollowedUsernamesAsync(
                    currentUsername,
                    cancellationToken
                );
                queryable = queryable.Where(x => followedUsernames.Contains(x.AuthorUsername));
            }

            if (!string.IsNullOrWhiteSpace(message.Tag))
            {
                var tag = await context.ArticleTags.FirstOrDefaultAsync(
                    x => x.TagId == message.Tag,
                    cancellationToken
                );
                if (tag != null)
                {
                    queryable = queryable.Where(x =>
                        x.ArticleTags.Select(y => y.TagId).Contains(tag.TagId)
                    );
                }
                else
                {
                    return new ArticlesEnvelope();
                }
            }

            if (!string.IsNullOrWhiteSpace(message.Author))
            {
                queryable = queryable.Where(x => x.AuthorUsername == message.Author);
            }

            if (!string.IsNullOrWhiteSpace(message.FavoritedUsername))
            {
                queryable = queryable.Where(x =>
                    x.ArticleFavorites.Any(y => y.Username == message.FavoritedUsername)
                );
            }

            var articles = await queryable
                .OrderByDescending(x => x.CreatedAt)
                .Skip(message.Offset ?? 0)
                .Take(message.Limit ?? 20)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            await articles.EnrichAuthorsAsync(
                profileQueryService,
                currentUsername,
                cancellationToken
            );

            return new ArticlesEnvelope { Articles = articles, ArticlesCount = queryable.Count() };
        }
    }
}
