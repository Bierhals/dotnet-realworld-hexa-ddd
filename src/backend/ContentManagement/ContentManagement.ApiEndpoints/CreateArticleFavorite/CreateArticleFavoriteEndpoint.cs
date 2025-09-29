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

namespace Conduit.ContentManagement.ApiEndpoints.CreateArticleFavorite;

internal sealed class CreateArticleFavoriteEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(string.Empty, HandleAsync)
            .WithSummary("Favorite an article")
            .WithDescription("Favorite an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints/#favorite-article\">Conduit spec for favorite article endpoint</a>")
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
    }

    private static Task<Results<Ok<SingleArticleResponse>, UnprocessableEntity<ValidationProblemDetails>>> HandleAsync([FromRoute] string slug, CancellationToken ct)
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
