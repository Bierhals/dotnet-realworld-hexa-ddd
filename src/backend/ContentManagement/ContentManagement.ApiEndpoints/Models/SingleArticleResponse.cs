namespace Conduit.ContentManagement.ApiEndpoints.Models;

public record SingleArticleResponse
{
    public required Article Article { get; init; }
}
