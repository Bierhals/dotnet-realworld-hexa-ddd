using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure.Security;
using MediatR;
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
        IMediator mediator,
        string slug,
        CancellationToken cancellationToken
    ) => mediator.Send(new Add.Command(slug), cancellationToken);

    private static Task<Articles.ArticleEnvelope> DeleteFavoriteAsync(
        IMediator mediator,
        string slug,
        CancellationToken cancellationToken
    ) => mediator.Send(new Delete.Command(slug), cancellationToken);
}
