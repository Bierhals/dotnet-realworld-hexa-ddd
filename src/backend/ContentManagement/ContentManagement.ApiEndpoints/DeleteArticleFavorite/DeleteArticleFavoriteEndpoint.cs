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

namespace Conduit.ContentManagement.ApiEndpoints.DeleteArticleFavorite;

internal sealed class DeleteArticleFavoriteEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(string.Empty, HandleAsync)
            .WithSummary("Unfavorite an article")
            .WithDescription("Unfavorite an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints/#unfavorite-article\">Conduit spec for Unfavorite article</a>")
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
