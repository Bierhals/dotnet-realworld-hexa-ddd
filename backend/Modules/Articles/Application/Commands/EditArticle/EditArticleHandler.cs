using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Application.Optional;
using ErrorOr;

namespace Conduit.Articles.Application.Commands.EditArticle;

public sealed class EditArticleHandler(
    IArticlesRepository articlesRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserAccessor currentUserAccessor) : ICommandHandler<EditArticleCommand, string>
{
    public Task<ErrorOr<string>> Handle(EditArticleCommand command, CancellationToken cancellationToken) =>
        CurrentUser.Resolve(currentUserAccessor)
            .ThenAsync(async editor =>
            {
                var article = await articlesRepository.GetBySlugAsync(
                    ArticleSlug.Rehydrate(command.Slug),
                    cancellationToken);

                return await article.ToErrorOr(Error.NotFound("Article.NotFound", "The article does not exist."))
                    .ThenAsync(a => ApplyEditAsync(a, editor, command, cancellationToken));
            });

    private async Task<ErrorOr<string>> ApplyEditAsync(
        Article article,
        Username editor,
        EditArticleCommand command,
        CancellationToken cancellationToken)
    {
        ArticleTitle? title = null;
        if (command.Title.IsSpecified)
        {
            var created = ArticleTitle.Create(command.Title.Value);
            if (created.IsError)
            {
                return created.Errors;
            }

            title = created.Value;
        }

        ArticleDescription? description = null;
        if (command.Description.IsSpecified)
        {
            var created = ArticleDescription.Create(command.Description.Value);
            if (created.IsError)
            {
                return created.Errors;
            }

            description = created.Value;
        }

        ArticleBody? body = null;
        if (command.Body.IsSpecified)
        {
            var created = ArticleBody.Create(command.Body.Value);
            if (created.IsError)
            {
                return created.Errors;
            }

            body = created.Value;
        }

        List<TagName>? tagNames = null;
        if (command.TagList.IsSpecified)
        {
            var created = TagNameList.Create(command.TagList.Value);
            if (created.IsError)
            {
                return created.Errors;
            }

            tagNames = created.Value;
        }

        return await article.Edit(editor, title, description, body, tagNames, DateTime.UtcNow)
            .ThenDoAsync(_ => unitOfWork.SaveChangesAsync(cancellationToken))
            .Then(_ => article.Slug.Value);
    }
}
