using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Host.WebApi.Domain;
using Conduit.Host.WebApi.Infrastructure;
using Conduit.Host.WebApi.Infrastructure.Errors;
using Conduit.Host.WebApi.Shared.RequestHandling;
using Conduit.Identity.Contracts.Queries;
using Conduit.Tags.Contracts.Catalog;

namespace Conduit.Host.WebApi.Features.Articles;

public class Create
{
    public class ArticleData
    {
        public required string Title { get; init; }

        public required string Description { get; init; }

        public required string Body { get; init; }

        public string[]? TagList { get; init; }
    }

    public record Command([Required] ArticleData Article) : ICommand<ArticleEnvelope>;

    public class Handler(
        ConduitContext context,
        ICurrentUserAccessor currentUserAccessor,
        IProfileQueryService profileQueryService,
        ITagCatalogService tagCatalogService
    ) : ICommandHandler<Command, ArticleEnvelope>
    {
        public async Task<ArticleEnvelope> Handle(
            Command message,
            CancellationToken cancellationToken
        )
        {
            var authorUsername = currentUserAccessor.GetCurrentUsername()!;
            var tagNames = (message.Article.TagList ?? []).Distinct(StringComparer.Ordinal).ToList();

            // The tag catalog is owned by the Tags module, so the article never creates tag rows
            // itself - it only announces that it now uses these tags.
            var reference = await tagCatalogService.ReferenceTagsAsync(tagNames, cancellationToken);
            if (reference.IsError)
            {
                throw new RestException(
                    HttpStatusCode.UnprocessableEntity,
                    new { TagList = reference.FirstError.Description }
                );
            }

            var article = new Article
            {
                AuthorUsername = authorUsername,
                Body = message.Article.Body,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Description = message.Article.Description,
                Title = message.Article.Title,
                Slug = message.Article.Title.GenerateSlug(),
            };
            await context.Articles.AddAsync(article, cancellationToken);

            await context.ArticleTags.AddRangeAsync(
                tagNames.Select(x => new ArticleTag { Article = article, TagId = x }),
                cancellationToken
            );

            await context.SaveChangesAsync(cancellationToken);

            await new[] { article }.EnrichAuthorsAsync(
                profileQueryService,
                authorUsername,
                cancellationToken
            );

            return new ArticleEnvelope(article);
        }
    }
}
