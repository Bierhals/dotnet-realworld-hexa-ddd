using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Conduit.ContentManagement.ApiEndpoints.Models;

public record GetArticlesRequest
{
    /// <summary>
    /// Filter by tag
    /// </summary>
    public string? Tag { get; init; }
    /// <summary>
    /// Filter by author (username)
    /// </summary>
    public string? Author { get; init; }
    /// <summary>
    /// Filter by favorites of a user (username)
    /// </summary>
    public string? Favorited { get; init; }
    /// <summary>
    /// The number of items to skip before starting to collect the result set.
    /// </summary>
    [Range(0, int.MaxValue), DefaultValue(0)]
    public int Offset { get; init; } = 0;
    /// <summary>
    /// The numbers of items to return.
    /// </summary>
    [Range(1, int.MaxValue), DefaultValue(20)]
    public int Limit { get; init; } = 20;
}
