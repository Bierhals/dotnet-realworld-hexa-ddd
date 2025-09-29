using Conduit.Shared.ApiEndpoints;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Conduit.ContentManagement.ApiEndpoints.DeleteArticle;

internal sealed class DeleteArticleEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/articles/{slug}", HandleAsync)
            .WithSummary("Delete an article")
            .WithDescription("Delete an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints/#delete-article\">Conduit spec for Delete Article endpoint</a>")
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags("Articles")
            .RequireAuthorization();
    }

    private static Task<Results<Ok, UnprocessableEntity<ValidationProblemDetails>>> HandleAsync([FromRoute] string slug, CancellationToken ct)
    {
        return Task.FromResult<Results<Ok, UnprocessableEntity<ValidationProblemDetails>>>(
            TypedResults.Ok());
    }
}
