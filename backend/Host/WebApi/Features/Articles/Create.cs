using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Host.WebApi.Domain;
using Conduit.Host.WebApi.Infrastructure;
using Conduit.Host.WebApi.Shared.RequestHandling;
using Conduit.Identity.Contracts.Queries;
using Microsoft.EntityFrameworkCore;

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
        IProfileQueryService profileQueryService
    ) : ICommandHandler<Command, ArticleEnvelope>
    {
        public async Task<ArticleEnvelope> Handle(
            Command message,
            CancellationToken cancellationToken
        )
        {
            var authorUsername = currentUserAccessor.GetCurrentUsername()!;
            var tags = new List<Tag>();
            foreach (var tag in (message.Article.TagList ?? Enumerable.Empty<string>()))
            {
                var t = await context.Tags.FindAsync(tag);
                if (t == null)
                {
                    t = new Tag { TagId = tag };
                    await context.Tags.AddAsync(t, cancellationToken);
                    //save immediately for reuse
                    await context.SaveChangesAsync(cancellationToken);
                }
                tags.Add(t);
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
                tags.Select(x => new ArticleTag { Article = article, Tag = x }),
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
