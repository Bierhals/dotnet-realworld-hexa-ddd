using Conduit.Articles.Application;
using Conduit.Shared.Application.Cqrs;

namespace Conduit.Articles.Application.Queries.ArticleDetails;

public sealed record ArticleDetailsQuery : IQuery<ArticleReadModel>
{
    public required string Slug { get; init; }
}
