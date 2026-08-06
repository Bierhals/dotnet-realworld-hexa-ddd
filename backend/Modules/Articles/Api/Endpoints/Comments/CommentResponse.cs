using System;
using System.Collections.Generic;
using Conduit.Articles.Api.Endpoints.Articles;

namespace Conduit.Articles.Api.Endpoints.Comments;

public sealed record CommentResponse
{
    public required int Id { get; init; }

    public required string Body { get; init; }

    public required DateTime CreatedAt { get; init; }

    public required DateTime UpdatedAt { get; init; }

    public required AuthorResponse Author { get; init; }
}

public sealed record CommentEnvelope(CommentResponse Comment);

public sealed record CommentsEnvelope(IReadOnlyCollection<CommentResponse> Comments);
