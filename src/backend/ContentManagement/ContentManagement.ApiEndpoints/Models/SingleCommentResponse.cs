using System.ComponentModel;

namespace Conduit.ContentManagement.ApiEndpoints.Models;

[Description("Single comment")]
public record SingleCommentResponse
{
    public required Comment Comment { get; set; }
}
