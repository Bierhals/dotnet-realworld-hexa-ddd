using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Articles.Application.Queries.ArticleDetails;

public sealed class ArticleDetailsHandler(
    IArticlesReadRepository articlesReadRepository,
    IProfileReader profileReader,
    ICurrentUserAccessor currentUserAccessor) : IQueryHandler<ArticleDetailsQuery, ArticleReadModel>
{
    public async Task<ErrorOr<ArticleReadModel>> Handle(ArticleDetailsQuery query, CancellationToken cancellationToken)
    {
        var viewerUsername = currentUserAccessor.GetCurrentUsername();
        var article = await articlesReadRepository.GetBySlugAsync(query.Slug, viewerUsername, cancellationToken);

        return await article.ToErrorOr(Error.NotFound("Article.NotFound", "The article does not exist."))
            .ThenAsync(async a => (await AuthorProfileResolver.ToReadModelsAsync(
                [a],
                profileReader,
                viewerUsername,
                cancellationToken)).First());
    }
}
