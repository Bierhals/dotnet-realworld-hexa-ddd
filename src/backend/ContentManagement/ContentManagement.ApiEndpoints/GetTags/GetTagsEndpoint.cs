using Conduit.Shared.ApiEndpoints;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Conduit.ContentManagement.ApiEndpoints.Models;

namespace Conduit.ContentManagement.ApiEndpoints.GetTags;

internal sealed class GetTagsEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(string.Empty, HandleAsync)
            .WithSummary("Get tags")
            .WithDescription("Get tags. Auth not required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints/#get-tags\">Conduit Spec for get tags endpoint</a>");
    }

    private static Task<Results<Ok<TagsResponse>, UnprocessableEntity<ValidationProblemDetails>>> HandleAsync(CancellationToken ct)
    {
        return Task.FromResult<Results<Ok<TagsResponse>, UnprocessableEntity<ValidationProblemDetails>>>(
            TypedResults.Ok(
                new TagsResponse
                {
                    Tags = new[] { "tag1", "tag2" }
                }));
    }
}
