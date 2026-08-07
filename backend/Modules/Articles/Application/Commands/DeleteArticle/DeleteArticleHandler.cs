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
    ICommentsRepository commentsRepository,
    IArticleFavoritesRepository favoritesRepository,
    IUnitOfWork unitOfWork,
    ITagCatalog tagCatalog,
    ICurrentUserAccessor currentUserAccessor) : ICommandHandler<DeleteArticleCommand>
{
    public async Task<ErrorOr<Success>> Handle(DeleteArticleCommand command, CancellationToken cancellationToken)
    {
        var requester = CurrentUser.Resolve(currentUserAccessor);
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

        // Read the tag names before the article is gone.
        var tagNames = TagNameList.ToValues(article.Tags);

        // Comments and favorites are their own aggregates and are deliberately independent of the
        // article in the database as well, so nothing cascades on its own. This use case is the
        // one place that knows neither of them can outlive the article they belong to.
        articlesRepository.Remove(article);
        await commentsRepository.RemoveAllForArticleAsync(article.Id, cancellationToken);
        await favoritesRepository.RemoveAllForArticleAsync(article.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // The article no longer uses these tags; the Tags module drops the ones that nothing
        // references anymore.
        await tagCatalog.ReleaseTagsAsync(tagNames, cancellationToken);

        return Result.Success;
    }
}
