using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Articles.Application.Commands.EditArticle;

public sealed class EditArticleHandler(
    IArticlesRepository articlesRepository,
    IUnitOfWork unitOfWork,
    ITagCatalog tagCatalog,
    ICurrentUserAccessor currentUserAccessor) : ICommandHandler<EditArticleCommand, string>
{
    public async Task<ErrorOr<string>> Handle(EditArticleCommand command, CancellationToken cancellationToken)
    {
        var editor = CurrentUser.Resolve(currentUserAccessor);
        if (editor.IsError)
        {
            return editor.Errors;
        }

        var article = await articlesRepository.GetBySlugAsync(
            ArticleSlug.Rehydrate(command.Slug),
            cancellationToken);
        if (article is null)
        {
            return Error.NotFound("Article.NotFound", "The article does not exist.");
        }

        ArticleTitle? title = null;
        if (command.Title is not null)
        {
            var created = ArticleTitle.Create(command.Title);
            if (created.IsError)
            {
                return created.Errors;
            }

            title = created.Value;
        }

        ArticleDescription? description = null;
        if (command.Description is not null)
        {
            var created = ArticleDescription.Create(command.Description);
            if (created.IsError)
            {
                return created.Errors;
            }

            description = created.Value;
        }

        ArticleBody? body = null;
        if (command.Body is not null)
        {
            var created = ArticleBody.Create(command.Body);
            if (created.IsError)
            {
                return created.Errors;
            }

            body = created.Value;
        }

        List<TagName>? tagNames = null;
        if (command.TagList is not null)
        {
            var created = TagNameList.Create(command.TagList);
            if (created.IsError)
            {
                return created.Errors;
            }

            tagNames = created.Value;
        }

        var tagChanges = article.Edit(editor.Value, title, description, body, tagNames, DateTime.UtcNow);
        if (tagChanges.IsError)
        {
            return tagChanges.Errors;
        }

        // Announce the tags this article starts using before persisting, so that a rejected tag
        // name does not leave a half-applied edit behind.
        var reference = await tagCatalog.ReferenceTagsAsync(
            TagNameList.ToValues(tagChanges.Value.Added),
            cancellationToken);
        if (reference.IsError)
        {
            return reference.Errors;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // The article no longer uses these tags; the Tags module drops the ones that nothing
        // references anymore.
        await tagCatalog.ReleaseTagsAsync(
            TagNameList.ToValues(tagChanges.Value.Removed),
            cancellationToken);

        return article.Slug.Value;
    }
}
