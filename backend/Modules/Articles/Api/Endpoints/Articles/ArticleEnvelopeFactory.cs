using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application;
using Conduit.Articles.Application.Queries.ArticleDetails;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Conduit.Articles.Api.Endpoints.Articles;

internal static class ArticleEnvelopeFactory
{
    /// <summary>
    /// Renders an article by slug. Commands return only the slug they affected and let this build
    /// the response, so that the shape of an article is assembled in exactly one place.
    /// </summary>
    public static async Task<Results<Ok<ArticleEnvelope>, ProblemHttpResult>> BuildAsync(
        string slug,
        ICqrsMediator mediator,
        CancellationToken cancellationToken)
    {
        var article = await mediator.Send(new ArticleDetailsQuery { Slug = slug }, cancellationToken);

        return article.IsError
            ? article.Errors.ToProblemResult()
            : TypedResults.Ok(new ArticleEnvelope(Create(article.Value)));
    }

    public static ArticleResponse Create(ArticleReadModel article) => new()
    {
        Slug = article.Slug,
        Title = article.Title,
        Description = article.Description,
        Body = article.Body,
        TagList = article.TagList,
        CreatedAt = article.CreatedAt,
        UpdatedAt = article.UpdatedAt,
        Favorited = article.Favorited,
        FavoritesCount = article.FavoritesCount,
        Author = Create(article.Author),
    };

    public static ArticlesEnvelope Create(ArticleListReadModel articles) =>
        new([.. articles.Articles.Select(Create)], articles.ArticlesCount);

    public static AuthorResponse Create(AuthorProfile author) => new()
    {
        Username = author.Username,
        Bio = author.Bio,
        Image = author.Image,
        Following = author.Following,
    };
}
