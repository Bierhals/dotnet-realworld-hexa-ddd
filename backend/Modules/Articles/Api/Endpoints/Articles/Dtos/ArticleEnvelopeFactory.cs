using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application;
using Conduit.Articles.Application.Queries.ArticleDetails;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Articles.Api.Endpoints.Articles.Dtos;

internal static class ArticleEnvelopeFactory
{
    /// <summary>
    /// Renders an article by slug. Commands return only the slug they affected and let this build
    /// the response, so that the shape of an article is assembled in exactly one place.
    /// </summary>
    public static Task<ErrorOr<ArticleEnvelope>> BuildAsync(
        string slug,
        ICqrsMediator mediator,
        CancellationToken cancellationToken) =>
        mediator.Send(new ArticleDetailsQuery { Slug = slug }, cancellationToken)
            .Then(article => new ArticleEnvelope(Create(article)));

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

    public static ArticleResponse.AuthorResponse Create(AuthorProfile author) => new()
    {
        Username = author.Username,
        Bio = author.Bio,
        Image = author.Image,
        Following = author.Following,
    };
}
