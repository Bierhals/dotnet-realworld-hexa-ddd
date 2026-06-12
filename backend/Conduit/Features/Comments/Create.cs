using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using Conduit.Shared.RequestHandling;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Comments;

public class Create
{
    public record CommentData([Required] string Body);

    public record Command([Required] Model Model, string Slug) : ICommand<CommentEnvelope>;

    public record Model([Required] CommentData Comment);

    public class Handler(ConduitContext context, ICurrentUserAccessor currentUserAccessor)
        : ICommandHandler<Command, CommentEnvelope>
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

            var author = await context.Persons.FirstAsync(
                x => x.Username == currentUserAccessor.GetCurrentUsername(),
                cancellationToken
            );

            var comment = new Comment
            {
                Author = author,
                Body = message.Model.Comment.Body ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            await context.Comments.AddAsync(comment, cancellationToken);

            article.Comments.Add(comment);

            await context.SaveChangesAsync(cancellationToken);

            return new CommentEnvelope(comment);
        }
    }
}
