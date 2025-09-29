namespace Conduit.ContentManagement.ApiEndpoints.Models;

public record NewArticleRequest
{
    public required NewArticle Article { get; init; }
}
