using System;
using Conduit.Articles.Api.Endpoints.Articles.Dtos;

namespace Conduit.Articles.Api.Endpoints.Comments.Dtos;

public sealed record CommentResponse
{
    public required int Id { get; init; }

    public required string Body { get; init; }

    public required DateTime CreatedAt { get; init; }

    public required DateTime UpdatedAt { get; init; }

    public required ArticleResponse.AuthorResponse Author { get; init; }
}
