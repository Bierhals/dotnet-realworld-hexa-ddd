using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.ContentManagement.ApiEndpoints.Models;
using Conduit.Shared.ApiEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Conduit.ContentManagement.ApiEndpoints.UpdateArticle;

internal sealed class UpdateArticleEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/{slug}", HandleAsync)
            .WithSummary("Update an article")
            .WithDescription("Update an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#update-article\">Conduit spec for Update Article endpoint</a>")
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
    }

    private static Task<Results<Ok<SingleArticleResponse>, UnprocessableEntity<ValidationProblemDetails>>> HandleAsync([FromRoute] string slug, [FromBody] UpdateArticleRequest request, CancellationToken ct)
    {
        return Task.FromResult<Results<Ok<SingleArticleResponse>, UnprocessableEntity<ValidationProblemDetails>>>(
            TypedResults.Ok(
                new SingleArticleResponse
                {
                    Article = new Article
                    {
                        Slug = "slug",
                        Title = "title",
                        Description = "description",
                        Body = "body",
                        TagList = new[] { "Test" },
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        Favorited = true,
                        FavoritesCount = 3,
                        Author = new Author
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
