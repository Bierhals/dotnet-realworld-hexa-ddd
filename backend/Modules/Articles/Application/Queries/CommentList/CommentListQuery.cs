using System.Collections.Generic;
using Conduit.Shared.Application.Cqrs;

namespace Conduit.Articles.Application.Queries.CommentList;

public sealed record CommentListQuery : IQuery<IReadOnlyCollection<CommentReadModel>>
{
    public required string Slug { get; init; }
}
