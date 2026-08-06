using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Articles.Application.Queries.ArticleList;

public sealed class ArticleListHandler(
    IArticlesReadRepository articlesReadRepository,
    IProfileReader profileReader,
    ICurrentUserAccessor currentUserAccessor) : IQueryHandler<ArticleListQuery, ArticleListReadModel>
{
    public async Task<ErrorOr<ArticleListReadModel>> Handle(ArticleListQuery query, CancellationToken cancellationToken)
    {
        var viewerUsername = currentUserAccessor.GetCurrentUsername();

        var filter = new ArticleListFilter
        {
            Tag = query.Tag,
            Author = query.Author,
            FavoritedBy = query.FavoritedBy,
            Limit = query.Limit,
            Offset = query.Offset,
        };

        var page = await articlesReadRepository.ListAsync(filter, viewerUsername, cancellationToken);

        var articles = await AuthorProfileResolver.ToReadModelsAsync(
            page.Articles,
            profileReader,
            viewerUsername,
            cancellationToken);

        return new ArticleListReadModel(articles, page.TotalCount);
    }
}
