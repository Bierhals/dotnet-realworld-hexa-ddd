using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Articles.Application.Queries.ArticleFeed;

public sealed class ArticleFeedHandler(
    IArticlesReadRepository articlesReadRepository,
    IProfileReader profileReader,
    ICurrentUserAccessor currentUserAccessor) : IQueryHandler<ArticleFeedQuery, ArticleListReadModel>
{
    public async Task<ErrorOr<ArticleListReadModel>> Handle(ArticleFeedQuery query, CancellationToken cancellationToken)
    {
        var viewerUsername = currentUserAccessor.GetCurrentUsername();
        if (viewerUsername is null)
        {
            return Error.Unauthorized("Article.FeedRequiresAuthentication", "A feed can only be read by an authenticated user.");
        }

        var followedAuthors = await profileReader.GetFollowedAuthorsAsync(viewerUsername, cancellationToken);

        var filter = new ArticleListFilter
        {
            Authors = followedAuthors,
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
