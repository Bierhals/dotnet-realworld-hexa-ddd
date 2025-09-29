using System.Collections.Generic;
using System.ComponentModel;

namespace Conduit.ContentManagement.ApiEndpoints.Models;

[Description("Multiple comments")]
public record MultipleCommentsResponse
{
    public required IEnumerable<Comment> Comments { get; init; }
}
