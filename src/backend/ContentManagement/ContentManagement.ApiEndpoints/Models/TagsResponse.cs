using System.Collections.Generic;
using System.ComponentModel;

namespace Conduit.ContentManagement.ApiEndpoints.Models;

[Description("Tags")]
public record TagsResponse
{
    public required IEnumerable<string> Tags { get; set; }
}
