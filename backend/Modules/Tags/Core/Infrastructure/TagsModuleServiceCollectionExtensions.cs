using System;
using Conduit.Tags.Core.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tags.Core.Infrastructure;

public static class TagsModuleServiceCollectionExtensions
{
    public static IServiceCollection AddTagsModule(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        services.AddTagsPersistence(configureDbContext);
        services.AddTagsApplication();

        return services;
    }
}
