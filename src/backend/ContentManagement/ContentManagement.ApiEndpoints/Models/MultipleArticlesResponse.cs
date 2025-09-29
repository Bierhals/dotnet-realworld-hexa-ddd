using System.Collections.Generic;

namespace Conduit.ContentManagement.ApiEndpoints.Models;

/// <summary>
/// Multiple articles
/// </summary>
public record MultipleArticlesResponse
{
    public required IEnumerable<Article> Articles { get; init; }
    public required int ArticlesCount { get; init; }
}
