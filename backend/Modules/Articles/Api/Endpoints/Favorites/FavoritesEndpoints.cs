using Conduit.Shared.Infrastructure.ApiEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Articles.Api.Endpoints.Favorites;

public static class FavoritesEndpoints
{
    public static IEndpointRouteBuilder MapFavoritesGroup(this IEndpointRouteBuilder endpoints)
    {
        var favorites = endpoints.MapGroup("/articles/{slug}/favorite")
            .WithTags("Favorites");

        favorites.AddEndpoint<FavoriteArticleEndpoint>();
        favorites.AddEndpoint<UnfavoriteArticleEndpoint>();

        return endpoints;
    }
}
