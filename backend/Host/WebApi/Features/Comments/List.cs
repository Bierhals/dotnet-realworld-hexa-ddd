using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Host.WebApi.Infrastructure;
using Conduit.Host.WebApi.Infrastructure.Errors;
using Conduit.Host.WebApi.Shared.RequestHandling;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Host.WebApi.Features.Comments;

public class List
{
    public record Query(string Slug) : IQuery<CommentsEnvelope>;

    public class Handler(ConduitContext context) : IQueryHandler<Query, CommentsEnvelope>
    {
        public async Task<CommentsEnvelope> Handle(
            Query message,
            CancellationToken cancellationToken
        )
        {
            var article = await context
                .Articles.Include(x => x.Comments)
                .ThenInclude(x => x.Author)
                .FirstOrDefaultAsync(x => x.Slug == message.Slug, cancellationToken);

            if (article == null)
            {
                throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Article = Constants.NOT_FOUND }
                );
            }

            return new CommentsEnvelope(article.Comments);
        }
    }
}
