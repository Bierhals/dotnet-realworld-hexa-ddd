using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure.Security;
using Conduit.Shared.RequestHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Features.Favorites;

public static class FavoritesEndpoints
{
    public static IEndpointRouteBuilder MapFavoritesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var favorites = endpoints.MapGroup("/articles/{slug}/favorite")
            .WithTags("Favorites")
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtIssuerOptions.Schemes });

        favorites.MapPost("", AddFavoriteAsync)
            .WithSummary("Favorite an article")
            .WithDescription("Favorite an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#favorite-article\">Conduit Spec for favorite article endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
        favorites.MapDelete("", DeleteFavoriteAsync)
            .WithSummary("Unfavorite an article")
            .WithDescription("Unfavorite an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#unfavorite-article\">Conduit Spec for unfavorite article endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }

    private static Task<Articles.ArticleEnvelope> AddFavoriteAsync(
        [Required]
        [Description("Slug of the article that you want to favorite")]
        string slug,
        ICommandHandler<Add.Command, Articles.ArticleEnvelope> commandHandler,
        CancellationToken cancellationToken
    ) => commandHandler.Handle(new Add.Command(slug), cancellationToken);

    private static Task<Articles.ArticleEnvelope> DeleteFavoriteAsync(
        [Required]
        [Description("Slug of the article that you want to unfavorite")]
        string slug,
        ICommandHandler<Delete.Command, Articles.ArticleEnvelope> commandHandler,
        CancellationToken cancellationToken
    ) => commandHandler.Handle(new Delete.Command(slug), cancellationToken);
}
