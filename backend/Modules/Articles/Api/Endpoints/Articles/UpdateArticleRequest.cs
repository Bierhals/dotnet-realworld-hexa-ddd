using System.ComponentModel.DataAnnotations;

namespace Conduit.Articles.Api.Endpoints.Articles;

public sealed record UpdateArticleRequest
{
    [Required]
    public required UpdateArticleData Article { get; init; }
}

/// <summary>
/// Every field is optional; the ones left out keep the value the article already has.
/// </summary>
public sealed record UpdateArticleData
{
    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? Body { get; init; }

    public string[]? TagList { get; init; }
}
