using Conduit.Articles.Api.Endpoints.Articles;
using Conduit.Articles.Api.Endpoints.Comments;
using Conduit.Articles.Api.Endpoints.Favorites;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Articles.Api;

public static class ArticlesApiEndpoints
{
    public static IEndpointRouteBuilder MapArticlesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapArticlesGroup();
        endpoints.MapCommentsGroup();
        endpoints.MapFavoritesGroup();

        return endpoints;
    }
}
