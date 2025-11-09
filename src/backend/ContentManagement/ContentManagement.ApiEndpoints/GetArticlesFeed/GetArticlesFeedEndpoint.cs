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

namespace Conduit.ContentManagement.ApiEndpoints.GetArticlesFeed;

internal sealed class GetArticlesFeedEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/feed", HandleAsync)
            .WithSummary("Get recent articles from users you follow")
            .WithDescription("Get most recent articles from users you follow. Use query parameters to limit. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/docs/specs/backend/endpoints/#registration\">Conduit Spec for registration endpoint</a>")
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
    }

    private static Task<Results<Ok<MultipleArticlesResponse>, UnprocessableEntity<ValidationProblemDetails>>> HandleAsync([AsParameters] GetArticlesFeedRequest filter, CancellationToken ct)
    {
        return Task.FromResult<Results<Ok<MultipleArticlesResponse>, UnprocessableEntity<ValidationProblemDetails>>>(
            TypedResults.Ok(
                new MultipleArticlesResponse
                {
                    ArticlesCount = 1,
                    Articles = new[]
                {
                    new Article
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
                }
                }));
    }
}
