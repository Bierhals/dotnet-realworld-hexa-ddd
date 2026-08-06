using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Articles.Application.Commands.DeleteArticle;

public sealed class DeleteArticleHandler(
    IArticlesRepository articlesRepository,
    IUnitOfWork unitOfWork,
    ITagCatalog tagCatalog,
    ICurrentUserAccessor currentUserAccessor) : ICommandHandler<DeleteArticleCommand>
{
    public async Task<ErrorOr<Success>> Handle(DeleteArticleCommand command, CancellationToken cancellationToken)
    {
        var requester = CurrentAuthor.Resolve(currentUserAccessor);
        if (requester.IsError)
        {
            return requester.Errors;
        }

        var article = await articlesRepository.GetBySlugAsync(
            ArticleSlug.Rehydrate(command.Slug),
            cancellationToken);
        if (article is null)
        {
            return Error.NotFound("Article.NotFound", "The article does not exist.");
        }

        var check = article.EnsureCanBeDeletedBy(requester.Value);
        if (check.IsError)
        {
            return check.Errors;
        }

        // Read the tag names before the article - and with it its tags - is gone.
        var tagNames = TagNameList.ToValues(article.TagNames);

        articlesRepository.Remove(article);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // The article no longer uses these tags; the Tags module drops the ones that nothing
        // references anymore.
        await tagCatalog.ReleaseTagsAsync(tagNames, cancellationToken);

        return Result.Success;
    }
}
