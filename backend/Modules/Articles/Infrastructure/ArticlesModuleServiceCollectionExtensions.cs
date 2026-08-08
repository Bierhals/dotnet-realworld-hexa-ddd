using System;
using Conduit.Articles.Application;
using Conduit.Articles.Infrastructure.Adapters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Articles.Infrastructure;

public static class ArticlesModuleServiceCollectionExtensions
{
    public static IServiceCollection AddArticlesModule(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        services.AddArticlesPersistence(configureDbContext);
        services.AddArticlesApplication();

        // The adapters onto the modules this one reads from. They are the only types in Articles
        // that reference another module's contracts.
        services.AddScoped<IProfileReader, IdentityProfileReaderAdapter>();
        services.AddScoped<ITagCatalog, TagsCatalogAdapter>();

        return services;
    }
}
