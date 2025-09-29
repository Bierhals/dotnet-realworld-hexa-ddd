namespace Conduit.ContentManagement.ApiEndpoints.Models;


public record UpdateArticleRequest
{
    public required UpdateArticle Article { get; init; }
}
