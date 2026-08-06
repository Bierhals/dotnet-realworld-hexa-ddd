using Conduit.Shared.Infrastructure.ApiEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Articles.Api.Endpoints.Articles;

public static class ArticlesEndpoints
{
    public static IEndpointRouteBuilder MapArticlesGroup(this IEndpointRouteBuilder endpoints)
    {
        var articles = endpoints.MapGroup("/articles")
            .WithTags("Articles");

        articles.AddEndpoint<ListArticlesEndpoint>();
        articles.AddEndpoint<FeedArticlesEndpoint>();
        articles.AddEndpoint<GetArticleEndpoint>();
        articles.AddEndpoint<CreateArticleEndpoint>();
        articles.AddEndpoint<UpdateArticleEndpoint>();
        articles.AddEndpoint<DeleteArticleEndpoint>();

        return endpoints;
    }
}
