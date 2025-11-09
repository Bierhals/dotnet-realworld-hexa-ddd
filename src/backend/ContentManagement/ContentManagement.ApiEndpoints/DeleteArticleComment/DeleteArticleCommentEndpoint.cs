using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.ApiEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Conduit.ContentManagement.ApiEndpoints.DeleteArticleComment;

internal sealed class DeleteArticleCommentEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}", HandleAsync)
            .WithSummary("Delete a comment for an article")
            .WithDescription("Delete a comment for an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints/#delete-comment\">Conduit spec for Delete Comment to an Article</a>")
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
    }

    private static Task<Results<Ok, UnprocessableEntity<ValidationProblemDetails>>> HandleAsync([FromRoute] string slug, [FromRoute] int id, CancellationToken ct)
    {
        return Task.FromResult<Results<Ok, UnprocessableEntity<ValidationProblemDetails>>>(
            TypedResults.Ok());
    }
}
