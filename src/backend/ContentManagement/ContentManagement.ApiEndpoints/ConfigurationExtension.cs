using System;
using Conduit.ContentManagement.ApiEndpoints.CreateArticle;
using Conduit.ContentManagement.ApiEndpoints.CreateArticleFavorite;
using Conduit.ContentManagement.ApiEndpoints.DeleteArticle;
using Conduit.ContentManagement.ApiEndpoints.DeleteArticleFavorite;
using Conduit.ContentManagement.ApiEndpoints.GetArticle;
using Conduit.ContentManagement.ApiEndpoints.GetArticles;
using Conduit.ContentManagement.ApiEndpoints.GetArticlesFeed;
using Conduit.ContentManagement.ApiEndpoints.GetTags;
using Conduit.ContentManagement.ApiEndpoints.UpdateArticle;
using Conduit.Shared.ApiEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.ContentManagement.ApiEndpoints;

public static class ConfigurationExtension
{
    public static void MapContentManagementEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapArticlesEndpoints();
        app.MapTagsEndpoints();
        app.MapFavoriteEndpoints();
    }

    private static void MapArticlesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/articles")
            .WithTags("Articles");

        group.AddEndpoint<GetArticleEndpoint>();
        group.AddEndpoint<GetArticlesEndpoint>();
        group.AddEndpoint<DeleteArticleEndpoint>();
        group.AddEndpoint<GetArticlesFeedEndpoint>();
        group.AddEndpoint<CreateArticleEndpoint>();
        group.AddEndpoint<UpdateArticleEndpoint>();
    }

    private static void MapTagsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tags")
            .WithTags("Tags");

        group.AddEndpoint<GetTagsEndpoint>();
    }

    private static void MapFavoriteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/articles/{slug}/favorite")
            .WithTags("Favorites");

        group.AddEndpoint<CreateArticleFavoriteEndpoint>();
        group.AddEndpoint<DeleteArticleFavoriteEndpoint>();
    }

    public static void ConfigureContentManagementJsonOptions(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, ContentManagementSerializerContext.Default);
        });
    }
}
