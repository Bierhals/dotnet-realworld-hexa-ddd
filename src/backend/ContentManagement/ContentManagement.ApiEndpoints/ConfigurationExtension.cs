using System;
using Conduit.ContentManagement.ApiEndpoints.DeleteArticle;
using Conduit.Shared.ApiEndpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.ContentManagement.ApiEndpoints;

public static class ConfigurationExtension
{
    public static void MapContentManagementEndpoints(this IEndpointRouteBuilder app)
    {
        app.AddEndpoint<DeleteArticleEndpoint>();
    }

    /*public static void ConfigureContentManagementJsonOptions(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, ContentManagementSerializerContext.Default);
        });
    }*/
}
