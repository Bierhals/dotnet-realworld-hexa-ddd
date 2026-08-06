using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Articles.Application.Commands.UnfavoriteArticle;

public sealed class UnfavoriteArticleHandler(
    IArticlesRepository articlesRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserAccessor currentUserAccessor) : ICommandHandler<UnfavoriteArticleCommand>
{
    public async Task<ErrorOr<Success>> Handle(UnfavoriteArticleCommand command, CancellationToken cancellationToken)
    {
        var user = CurrentAuthor.Resolve(currentUserAccessor);
        if (user.IsError)
        {
            return user.Errors;
        }

        var article = await articlesRepository.GetBySlugAsync(
            ArticleSlug.Rehydrate(command.Slug),
            cancellationToken);
        if (article is null)
        {
            return Error.NotFound("Article.NotFound", "The article does not exist.");
        }

        article.Unfavorite(user.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
