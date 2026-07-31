using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Host.WebApi.Infrastructure;
using Conduit.Host.WebApi.Infrastructure.Errors;
using Conduit.Host.WebApi.Shared.RequestHandling;
using Conduit.Identity.Contracts.Queries;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Host.WebApi.Features.Comments;

public class List
{
    public record Query(string Slug) : IQuery<CommentsEnvelope>;

    public class Handler(
        ConduitContext context,
        ICurrentUserAccessor currentUserAccessor,
        IProfileQueryService profileQueryService
    ) : IQueryHandler<Query, CommentsEnvelope>
    {
        public async Task<CommentsEnvelope> Handle(
            Query message,
            CancellationToken cancellationToken
        )
        {
            var article = await context
                .Articles.Include(x => x.Comments)
                .FirstOrDefaultAsync(x => x.Slug == message.Slug, cancellationToken);

            if (article == null)
            {
                throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Article = Constants.NOT_FOUND }
                );
            }

            await article.Comments.EnrichAuthorsAsync(
                profileQueryService,
                currentUserAccessor.GetCurrentUsername(),
                cancellationToken
            );

            return new CommentsEnvelope(article.Comments);
        }
    }
}
