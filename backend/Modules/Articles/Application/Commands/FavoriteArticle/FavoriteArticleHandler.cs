using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Articles.Application.Commands.FavoriteArticle;

public sealed class FavoriteArticleHandler(
    IArticlesRepository articlesRepository,
    IArticleFavoritesRepository favoritesRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserAccessor currentUserAccessor) : ICommandHandler<FavoriteArticleCommand>
{
    public Task<ErrorOr<Success>> Handle(FavoriteArticleCommand command, CancellationToken cancellationToken) =>
        CurrentUser.Resolve(currentUserAccessor)
            .ThenAsync(async user =>
            {
                var articleId = await articlesRepository.GetIdBySlugAsync(
                    ArticleSlug.Rehydrate(command.Slug),
                    cancellationToken);

                return await articleId.ToErrorOr(Error.NotFound("Article.NotFound", "The article does not exist."))
                    .ThenDoAsync(async id =>
                    {
                        // Favoriting an article that is already favorited must not count twice.
                        var existing = await favoritesRepository.GetAsync(id, user, cancellationToken);
                        if (existing is not null)
                        {
                            return;
                        }

                        favoritesRepository.Add(ArticleFavorite.Create(id, user));
                        await unitOfWork.SaveChangesAsync(cancellationToken);
                    })
                    .Then(_ => Result.Success);
            });
}
