using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Host.WebApi.Domain;
using Conduit.Host.WebApi.Infrastructure;
using Conduit.Host.WebApi.Infrastructure.Errors;
using Conduit.Host.WebApi.Shared.RequestHandling;
using Conduit.Identity.Contracts.Queries;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Host.WebApi.Features.Comments;

public class Create
{
    public record CommentData([Required] string Body);

    public record Command([Required] Model Model, string Slug) : ICommand<CommentEnvelope>;

    public record Model([Required] CommentData Comment);

    public class Handler(
        ConduitContext context,
        ICurrentUserAccessor currentUserAccessor,
        IProfileQueryService profileQueryService
    ) : ICommandHandler<Command, CommentEnvelope>
    {
        public async Task<CommentEnvelope> Handle(
            Command message,
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

            var authorUsername = currentUserAccessor.GetCurrentUsername()!;

            var comment = new Comment
            {
                AuthorUsername = authorUsername,
                Body = message.Model.Comment.Body ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            await context.Comments.AddAsync(comment, cancellationToken);

            article.Comments.Add(comment);

            await context.SaveChangesAsync(cancellationToken);

            await new[] { comment }.EnrichAuthorsAsync(
                profileQueryService,
                authorUsername,
                cancellationToken
            );

            return new CommentEnvelope(comment);
        }
    }
}
