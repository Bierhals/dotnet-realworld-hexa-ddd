using System;

using Conduit.RestAPI.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace Conduit.RestAPI.Controllers;

[ApiController]
[Route("articles")]
[Tags("Articles")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class ArticlesController : ControllerBase
{
    /// <summary>
    /// Get recent articles from users you follow
    /// </summary>
    /// <remarks>
    /// Get most recent articles from users you follow. Use query parameters to limit. Auth is required<br/><a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints/#registration">Conduit Spec for registration endpoint</a>
    /// </remarks>
    /// <returns></returns>
    /// <param name="offset">The number of items to skip before starting to collect the result set.</param>
    /// <param name="limit">The numbers of items to return.</param>
    /// <response code="200">Successfully queryied articles</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="422">Article request is invalid</response>
    [HttpGet("feed")]
    [ProducesResponseType<MultipleArticlesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetArticlesFeed(int offset, int limit)
    {
        return Ok(
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
                        tagList = new[] { "Test" },
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
                }
            });
    }
}