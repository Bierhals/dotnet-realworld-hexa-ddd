using System.Net;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Features.Articles;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using Microsoft.EntityFrameworkCore;
using Conduit.Shared.RequestHandling;

namespace Conduit.Features.Favorites;

public class Delete
{
    public record Command([Required] string Slug) : ICommand<ArticleEnvelope>;

    public class Handler(ConduitContext context, ICurrentUserAccessor currentUserAccessor)
        : ICommandHandler<Command, ArticleEnvelope>
    {
        public async Task<ArticleEnvelope> Handle(
            Command message,
            CancellationToken cancellationToken
        )
        {
            var article =
                await context.Articles.FirstOrDefaultAsync(
                    x => x.Slug == message.Slug,
                    cancellationToken
                )
                ?? throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Article = Constants.NOT_FOUND }
                );

            var person = await context.Persons.FirstOrDefaultAsync(
                x => x.Username == currentUserAccessor.GetCurrentUsername(),
                cancellationToken
            );
            if (person is null)
            {
                throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Article = Constants.NOT_FOUND }
                );
            }

            var favorite = await context.ArticleFavorites.FirstOrDefaultAsync(
                x => x.ArticleId == article.ArticleId && x.PersonId == person.PersonId,
                cancellationToken
            );

            if (favorite != null)
            {
                context.ArticleFavorites.Remove(favorite);
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

            return new ArticleEnvelope(article);
        }
    }
}
