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

        favorites.MapPost("", AddFavoriteAsync);
        favorites.MapDelete("", DeleteFavoriteAsync);

        return endpoints;
    }

    private static Task<Articles.ArticleEnvelope> AddFavoriteAsync(
        ICommandHandler<Add.Command, Articles.ArticleEnvelope> commandHandler,
        [Required] string slug,
        CancellationToken cancellationToken
    ) => commandHandler.Handle(new Add.Command(slug), cancellationToken);

    private static Task<Articles.ArticleEnvelope> DeleteFavoriteAsync(
        ICommandHandler<Delete.Command, Articles.ArticleEnvelope> commandHandler,
        [Required] string slug,
        CancellationToken cancellationToken
    ) => commandHandler.Handle(new Delete.Command(slug), cancellationToken);
}
