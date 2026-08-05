using System;
using System.Collections.Generic;
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
using Microsoft.EntityFrameworkCore;

namespace Conduit.Host.WebApi.Features.Articles;

public class Edit
{
    public record ArticleData(string? Title, string? Description, string? Body, string[]? TagList);

    public record Command([Required] Model Model, string Slug) : ICommand<ArticleEnvelope>;

    public record Model([property: Required] ArticleData Article);

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
            var article = await context
                .Articles.Include(x => x.ArticleTags) // include also the article tags since they also need to be updated
                .Where(x => x.Slug == message.Slug)
                .FirstOrDefaultAsync(cancellationToken);

            if (article == null)
            {
                throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Article = Constants.NOT_FOUND }
                );
            }

            article.Description = message.Model.Article.Description ?? article.Description;
            article.Body = message.Model.Article.Body ?? article.Body;
            article.Title = message.Model.Article.Title ?? article.Title;
            article.Slug = article.Title.GenerateSlug();

            // list of currently saved article tags for the given article
            var articleTagList = message.Model.Article.TagList ?? Enumerable.Empty<string>();

            var articleTagsToCreate = GetArticleTagsToCreate(article, articleTagList);
            var articleTagsToDelete = GetArticleTagsToDelete(article, articleTagList);

            if (
                context.ChangeTracker.Entries().First(x => x.Entity == article).State
                    == EntityState.Modified
                || articleTagsToCreate.Count != 0
                || articleTagsToDelete.Count != 0
            )
            {
                article.UpdatedAt = DateTime.UtcNow;
            }

            // The tag catalog is owned by the Tags module: announce the tags this article starts
            // using and give up the ones it no longer uses.
            var reference = await tagCatalogService.ReferenceTagsAsync(
                [.. articleTagsToCreate.Where(x => x.TagId is not null).Select(x => x.TagId!)],
                cancellationToken
            );
            if (reference.IsError)
            {
                throw new RestException(
                    HttpStatusCode.UnprocessableEntity,
                    new { TagList = reference.FirstError.Description }
                );
            }

            await tagCatalogService.ReleaseTagsAsync(
                [.. articleTagsToDelete.Where(x => x.TagId is not null).Select(x => x.TagId!)],
                cancellationToken
            );

            // add the new article tags
            await context.ArticleTags.AddRangeAsync(articleTagsToCreate, cancellationToken);

            // delete the tags that do not exist anymore
            context.ArticleTags.RemoveRange(articleTagsToDelete);

            await context.SaveChangesAsync(cancellationToken);

            article = await context
                .Articles.GetAllData()
                .Where(x => x.Slug == article.Slug)
                .FirstOrDefaultAsync(x => x.ArticleId == article.ArticleId, cancellationToken);
            if (article is null)
            {
                throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Article = Constants.NOT_FOUND }
                );
            }

            await new[] { article }.EnrichAuthorsAsync(
                profileQueryService,
                currentUserAccessor.GetCurrentUsername(),
                cancellationToken
            );

            return new ArticleEnvelope(article);
        }

        /// <summary>
        /// check which article tags need to be added
        /// </summary>
        private static List<ArticleTag> GetArticleTagsToCreate(
            Article article,
            IEnumerable<string> articleTagList
        )
        {
            var articleTagsToCreate = new List<ArticleTag>();
            foreach (var tag in articleTagList)
            {
                var at = article.ArticleTags?.FirstOrDefault(t => t.TagId == tag);
                if (at == null)
                {
                    at = new ArticleTag
                    {
                        Article = article,
                        ArticleId = article.ArticleId,
                        TagId = tag,
                    };
                    articleTagsToCreate.Add(at);
                }
            }

            return articleTagsToCreate;
        }

        /// <summary>
        /// check which article tags need to be deleted
        /// </summary>
        private static List<ArticleTag> GetArticleTagsToDelete(
            Article article,
            IEnumerable<string> articleTagList
        )
        {
            var articleTagsToDelete = new List<ArticleTag>();
            foreach (var tag in article.ArticleTags)
            {
                var at = articleTagList.FirstOrDefault(t => t == tag.TagId);
                if (at == null)
                {
                    articleTagsToDelete.Add(tag);
                }
            }

            return articleTagsToDelete;
        }
    }
}
