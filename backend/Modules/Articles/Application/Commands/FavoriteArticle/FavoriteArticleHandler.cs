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
    public async Task<ErrorOr<Success>> Handle(FavoriteArticleCommand command, CancellationToken cancellationToken)
    {
        var user = CurrentAuthor.Resolve(currentUserAccessor);
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

        // Favoriting an article that is already favorited must not count twice.
        var existing = await favoritesRepository.GetAsync(articleId.Value, user.Value, cancellationToken);
        if (existing is not null)
        {
            return Result.Success;
        }

        favoritesRepository.Add(ArticleFavorite.Create(articleId.Value, user.Value));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
