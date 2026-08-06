using Conduit.Shared.Application.Cqrs;

namespace Conduit.Articles.Application.Queries.ArticleFeed;

public sealed record ArticleFeedQuery : IQuery<ArticleListReadModel>
{
    public required int Limit { get; init; }

    public required int Offset { get; init; }
}
