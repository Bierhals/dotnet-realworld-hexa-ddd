using System.ComponentModel.DataAnnotations;

namespace Conduit.Articles.Api.Endpoints.Articles;

public sealed record CreateArticleRequest
{
    [Required]
    public required CreateArticleData Article { get; init; }
}

public sealed record CreateArticleData
{
    [Required]
    public required string Title { get; init; }

    [Required]
    public required string Description { get; init; }

    [Required]
    public required string Body { get; init; }

    public string[]? TagList { get; init; }
}
