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

namespace Conduit.ContentManagement.ApiEndpoints.CreateArticleComment;

internal sealed class CreateArticleCommentEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(string.Empty, HandleAsync)
            .WithSummary("Create a comment for an article")
            .WithDescription("Create a comment for an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#add-comments-to-an-article\">Conduit spec for Add Comments to an Article</a>")
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
    }

    private static Task<Results<Ok<SingleCommentResponse>, UnprocessableEntity<ValidationProblemDetails>>> HandleAsync([FromRoute] string slug, [FromBody] NewCommentRequest request, CancellationToken ct)
    {
        return Task.FromResult<Results<Ok<SingleCommentResponse>, UnprocessableEntity<ValidationProblemDetails>>>(
            TypedResults.Ok(
                new SingleCommentResponse
            {
                Comment = new Comment
                {
                    Id = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Body = "body",
                    Author = new Profile
                    {
                        Username = "username",
                        Bio = "bio",
                        Image = "image",
                        Following = false
                    }
                }
            }));
    }
}
