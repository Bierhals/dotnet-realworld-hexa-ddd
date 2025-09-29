using System.Collections.Generic;
using System.ComponentModel;

namespace Conduit.ContentManagement.ApiEndpoints.Models;

[Description("Multiple articles")]
public record MultipleArticlesResponse
{
    public required IEnumerable<Article> Articles { get; init; }
    public required int ArticlesCount { get; init; }
}
