using System.ComponentModel.DataAnnotations;

namespace Conduit.Articles.Api.Endpoints.Comments;

public sealed record CreateCommentRequest
{
    [Required]
    public required CreateCommentData Comment { get; init; }
}

public sealed record CreateCommentData
{
    [Required]
    public required string Body { get; init; }
}
