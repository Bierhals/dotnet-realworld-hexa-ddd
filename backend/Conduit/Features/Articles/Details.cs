using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using Conduit.Shared.RequestHandling;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Articles;

public class Details
{
    public record Query([Required] string Slug) : IQuery<ArticleEnvelope>;

    public class Handler(ConduitContext context) : IQueryHandler<Query, ArticleEnvelope>
    {
        public async Task<ArticleEnvelope> Handle(
            Query message,
            CancellationToken cancellationToken
        )
        {
            var article = await context
                .Articles.GetAllData()
                .FirstOrDefaultAsync(x => x.Slug == message.Slug, cancellationToken);

            if (article == null)
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
