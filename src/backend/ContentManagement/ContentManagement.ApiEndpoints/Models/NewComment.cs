namespace Conduit.ContentManagement.ApiEndpoints.Models;

public record NewComment
{
    public required string Body { get; init; }
}
