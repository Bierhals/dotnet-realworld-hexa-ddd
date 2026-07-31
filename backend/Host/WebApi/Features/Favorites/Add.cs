using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Host.WebApi.Domain;
using Conduit.Host.WebApi.Features.Articles;
using Conduit.Host.WebApi.Infrastructure;
using Conduit.Host.WebApi.Infrastructure.Errors;
using Conduit.Host.WebApi.Shared.RequestHandling;
using Conduit.Identity.Contracts.Queries;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Host.WebApi.Features.Favorites;

public class Add
{
    public record Command([Required] string Slug) : ICommand<ArticleEnvelope>;

    public class Handler(
        ConduitContext context,
        ICurrentUserAccessor currentUserAccessor,
        IProfileQueryService profileQueryService
    ) : ICommandHandler<Command, ArticleEnvelope>
    {
        public async Task<ArticleEnvelope> Handle(
            Command message,
            CancellationToken cancellationToken
        )
        {
            var article = await context.Articles.FirstOrDefaultAsync(
                x => x.Slug == message.Slug,
                cancellationToken
            );

            if (article == null)
            {
                throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Article = Constants.NOT_FOUND }
                );
            }

            var username = currentUserAccessor.GetCurrentUsername()!;

            var favorite = await context.ArticleFavorites.FirstOrDefaultAsync(
                x => x.ArticleId == article.ArticleId && x.Username == username,
                cancellationToken
            );

            if (favorite == null)
            {
                favorite = new ArticleFavorite
                {
                    Article = article,
                    ArticleId = article.ArticleId,
                    Username = username,
                };
                await context.ArticleFavorites.AddAsync(favorite, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }

            article = await context
                .Articles.GetAllData()
                .FirstOrDefaultAsync(x => x.ArticleId == article.ArticleId, cancellationToken);
            if (article is null)
            {
                throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Article = Constants.NOT_FOUND }
                );
            }

            await new[] { article }.EnrichAuthorsAsync(
                profileQueryService,
                username,
                cancellationToken
            );

            return new ArticleEnvelope(article);
        }
    }
}
