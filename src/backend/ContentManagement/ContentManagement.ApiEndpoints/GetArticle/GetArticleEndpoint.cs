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

namespace Conduit.ContentManagement.ApiEndpoints.GetArticle;

internal sealed class GetArticleEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/{slug}", HandleAsync)
            .WithSummary("Get an article")
            .WithDescription("Get an article. Auth not required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints/#get-article\">Conduit spec for Get Article endpoint</a>");
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
