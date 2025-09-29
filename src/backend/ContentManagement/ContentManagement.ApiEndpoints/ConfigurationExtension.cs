using System;
using Conduit.ContentManagement.ApiEndpoints.CreateArticle;
using Conduit.ContentManagement.ApiEndpoints.DeleteArticle;
using Conduit.ContentManagement.ApiEndpoints.GetArticle;
using Conduit.ContentManagement.ApiEndpoints.GetArticles;
using Conduit.ContentManagement.ApiEndpoints.GetArticlesFeed;
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
        var group = app.MapGroup("/articles")
            .WithTags("Articles");

        group.MapPublicEndpoints();
        group.MapAuthorizedEndpoints();
    }

    private static void MapPublicEndpoints(this IEndpointRouteBuilder app)
    {
        app.AddEndpoint<GetArticleEndpoint>();
        app.AddEndpoint<GetArticlesEndpoint>();
    }

    private static void MapAuthorizedEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .RequireAuthorization();

        group.AddEndpoint<DeleteArticleEndpoint>();
        group.AddEndpoint<GetArticlesFeedEndpoint>();
        group.AddEndpoint<CreateArticleEndpoint>();
        group.AddEndpoint<UpdateArticleEndpoint>();
    }

    public static void ConfigureContentManagementJsonOptions(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, ContentManagementSerializerContext.Default);
        });
    }
}
