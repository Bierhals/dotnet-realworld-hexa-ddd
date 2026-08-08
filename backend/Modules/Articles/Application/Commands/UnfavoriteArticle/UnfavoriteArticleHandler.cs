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
    public Task<ErrorOr<Success>> Handle(UnfavoriteArticleCommand command, CancellationToken cancellationToken) =>
        CurrentUser.Resolve(currentUserAccessor)
            .ThenAsync(async user =>
            {
                var articleId = await articlesRepository.GetIdBySlugAsync(
                    ArticleSlug.Rehydrate(command.Slug),
                    cancellationToken);

                return await articleId.ToErrorOr(Error.NotFound("Article.NotFound", "The article does not exist."))
                    .ThenDoAsync(async id =>
                    {
                        // Giving up an article that was never favorited is not an error.
                        var favorite = await favoritesRepository.GetAsync(id, user, cancellationToken);
                        if (favorite is null)
                        {
                            return;
                        }

                        favorite.Remove();
                        favoritesRepository.Remove(favorite);
                        await unitOfWork.SaveChangesAsync(cancellationToken);
                    })
                    .Then(_ => Result.Success);
            });
}
