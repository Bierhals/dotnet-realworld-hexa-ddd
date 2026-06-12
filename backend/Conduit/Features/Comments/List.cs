using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using Conduit.Shared.RequestHandling;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Comments;

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
