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

namespace Conduit.ContentManagement.ApiEndpoints.GetArticles;

internal sealed class GetArticlesEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(string.Empty, HandleAsync)
            .WithSummary("Get recent articles globally")
            .WithDescription("Get most recent articles globally. Use query parameters to filter results. Auth is optional<br/><a href=\"https://realworld-docs.netlify.app/docs/specs/backend/endpoints#list-articles\">Conduit spec for List Articles Endpoint</a>");
    }

    private static Task<Results<Ok<MultipleArticlesResponse>, UnprocessableEntity<ValidationProblemDetails>>> HandleAsync([AsParameters] GetArticlesRequest filter, CancellationToken ct)
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
