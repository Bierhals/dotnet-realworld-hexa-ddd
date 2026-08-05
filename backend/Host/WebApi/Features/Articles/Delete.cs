using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Host.WebApi.Infrastructure;
using Conduit.Host.WebApi.Infrastructure.Errors;
using Conduit.Host.WebApi.Shared.RequestHandling;
using Conduit.Tags.Contracts.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Host.WebApi.Features.Articles;

public class Delete
{
    public record Command([Required] string Slug) : ICommand;

    public class Handler(ConduitContext context, ITagCatalogService tagCatalogService)
        : ICommandHandler<Command>
    {
        public async Task Handle(Command message, CancellationToken cancellationToken)
        {
            var article =
                await context
                    .Articles.Include(x => x.ArticleTags)
                    .FirstOrDefaultAsync(x => x.Slug == message.Slug, cancellationToken)
                ?? throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Article = Constants.NOT_FOUND }
                );

            // Read the tag names before the article - and with it its ArticleTag rows - is gone.
            var tagNames = article.TagList;

            context.Articles.Remove(article);
            await context.SaveChangesAsync(cancellationToken);

            // The article no longer uses these tags; the Tags module drops the ones that nothing
            // references anymore.
            await tagCatalogService.ReleaseTagsAsync(tagNames, cancellationToken);
        }
    }
}
