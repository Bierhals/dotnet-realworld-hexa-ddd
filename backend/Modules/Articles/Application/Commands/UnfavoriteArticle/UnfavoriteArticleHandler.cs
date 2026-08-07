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
    IArticleFavoritesRepository favoritesRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserAccessor currentUserAccessor) : ICommandHandler<UnfavoriteArticleCommand>
{
    public async Task<ErrorOr<Success>> Handle(UnfavoriteArticleCommand command, CancellationToken cancellationToken)
    {
        var user = CurrentUser.Resolve(currentUserAccessor);
        if (user.IsError)
        {
            return user.Errors;
        }

        var articleId = await articlesRepository.GetIdBySlugAsync(
            ArticleSlug.Rehydrate(command.Slug),
            cancellationToken);
        if (articleId is null)
        {
            return Error.NotFound("Article.NotFound", "The article does not exist.");
        }

        // Giving up an article that was never favorited is not an error.
        var favorite = await favoritesRepository.GetAsync(articleId.Value, user.Value, cancellationToken);
        if (favorite is null)
        {
            return Result.Success;
        }

        favorite.Remove();
        favoritesRepository.Remove(favorite);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
