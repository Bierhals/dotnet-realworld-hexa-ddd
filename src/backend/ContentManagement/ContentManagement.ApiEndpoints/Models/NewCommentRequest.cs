namespace Conduit.ContentManagement.ApiEndpoints.Models;

public record NewCommentRequest
{
    public required NewComment Comment { get; init; }
}
