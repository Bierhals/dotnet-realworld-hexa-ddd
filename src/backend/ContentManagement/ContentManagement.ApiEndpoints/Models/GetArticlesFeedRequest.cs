using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Conduit.ContentManagement.ApiEndpoints.Models;

public record class GetArticlesFeedRequest
{
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
