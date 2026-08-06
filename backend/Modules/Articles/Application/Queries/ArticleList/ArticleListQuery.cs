using Conduit.Shared.Application.Cqrs;

namespace Conduit.Articles.Application.Queries.ArticleList;

public sealed record ArticleListQuery : IQuery<ArticleListReadModel>
{
    public string? Tag { get; init; }

    public string? Author { get; init; }

    public string? FavoritedBy { get; init; }

    public required int Limit { get; init; }

    public required int Offset { get; init; }
}
