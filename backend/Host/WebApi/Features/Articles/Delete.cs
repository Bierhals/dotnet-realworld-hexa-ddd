using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Host.WebApi.Infrastructure;
using Conduit.Host.WebApi.Infrastructure.Errors;
using Conduit.Host.WebApi.Shared.RequestHandling;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Host.WebApi.Features.Articles;

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
