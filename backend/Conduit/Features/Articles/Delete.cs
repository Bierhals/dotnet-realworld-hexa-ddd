using System.Net;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using Microsoft.EntityFrameworkCore;
using Conduit.Shared.RequestHandling;

namespace Conduit.Features.Articles;

public class Delete
{
    public record Command([Required] string Slug) : ICommand;

    public class Handler(ConduitContext context) : ICommandHandler<Command>
    {
        public async Task Handle(Command message, CancellationToken cancellationToken)
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

            context.Articles.Remove(article);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
