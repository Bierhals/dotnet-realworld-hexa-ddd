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

namespace Conduit.ContentManagement.ApiEndpoints.GetArticleComments;

internal sealed class GetArticleCommentsEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(string.Empty, HandleAsync)
            .WithSummary("Get comments for an article")
            .WithDescription("Get the comments for an article. Auth is optional<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints/#get-comments-from-an-article\">Conduit spec for Get All Comments for and Article</a>");
    }

    private static Task<Results<Ok<MultipleCommentsResponse>, UnprocessableEntity<ValidationProblemDetails>>> HandleAsync([FromRoute] string slug, CancellationToken ct)
    {
        return Task.FromResult<Results<Ok<MultipleCommentsResponse>, UnprocessableEntity<ValidationProblemDetails>>>(
            TypedResults.Ok(
                new MultipleCommentsResponse
            {
                Comments = new[]
                {
                    new Comment
                    {
                        Id = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        Body = "body",
                        Author = new Author
                        {
                            Username = "username",
                            Bio = "bio",
                            Image = "image",
                            Following = false
                        }
                    }
                }
            }));
    }
}
