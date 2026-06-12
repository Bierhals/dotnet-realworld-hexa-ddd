using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using Conduit.Shared.RequestHandling;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Comments;

public class Delete
{
    public record Command([Required] string Slug, int Id) : ICommand;

    public class Handler(ConduitContext context) : ICommandHandler<Command>
    {
        public async Task Handle(Command message, CancellationToken cancellationToken)
        {
            var article =
                await context
                    .Articles.Include(x => x.Comments)
                    .FirstOrDefaultAsync(x => x.Slug == message.Slug, cancellationToken)
                ?? throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Article = Constants.NOT_FOUND }
                );

            var comment =
                article.Comments.FirstOrDefault(x => x.CommentId == message.Id)
                ?? throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Comment = Constants.NOT_FOUND }
                );

            context.Comments.Remove(comment);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
