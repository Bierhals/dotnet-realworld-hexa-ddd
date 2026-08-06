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

        var article = await articlesRepository.GetBySlugAsync(
            ArticleSlug.Rehydrate(command.Slug),
            cancellationToken);
        if (article is null)
        {
            return Error.NotFound("Article.NotFound", "The article does not exist.");
        }

        article.Favorite(user.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
