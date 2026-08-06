using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Articles.Application.Commands.CreateArticle;

public sealed class CreateArticleHandler(
    IArticlesRepository articlesRepository,
    IUnitOfWork unitOfWork,
    ITagCatalog tagCatalog,
    ICurrentUserAccessor currentUserAccessor) : ICommandHandler<CreateArticleCommand, string>
{
    public async Task<ErrorOr<string>> Handle(CreateArticleCommand command, CancellationToken cancellationToken)
    {
        var author = CurrentAuthor.Resolve(currentUserAccessor);
        if (author.IsError)
        {
            return author.Errors;
        }

        var title = ArticleTitle.Create(command.Title);
        if (title.IsError)
        {
            return title.Errors;
        }

        var description = ArticleDescription.Create(command.Description);
        if (description.IsError)
        {
            return description.Errors;
        }

        var body = ArticleBody.Create(command.Body);
        if (body.IsError)
        {
            return body.Errors;
        }

        var tagNames = TagNameList.Create(command.TagList);
        if (tagNames.IsError)
        {
            return tagNames.Errors;
        }

        // The tag catalog is owned by the Tags module, so the article never creates tag rows
        // itself - it only announces that it now uses these tags.
        var reference = await tagCatalog.ReferenceTagsAsync(
            TagNameList.ToValues(tagNames.Value),
            cancellationToken);
        if (reference.IsError)
        {
            return reference.Errors;
        }

        var article = Article.Publish(
            author.Value,
            title.Value,
            description.Value,
            body.Value,
            tagNames.Value,
            DateTime.UtcNow);

        articlesRepository.Add(article);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return article.Slug.Value;
    }
}
