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

namespace Conduit.ContentManagement.ApiEndpoints.CreateArticle;

internal sealed class CreateArticleEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(string.Empty, HandleAsync)
            .WithSummary("Create an article")
            .WithDescription("Create an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints/#create-article\">Conduit Spec for create article endpoint</a>")
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static Task<Results<Ok<SingleArticleResponse>, UnprocessableEntity<ValidationProblemDetails>>> HandleAsync([FromBody] NewArticleRequest request, CancellationToken ct)
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
