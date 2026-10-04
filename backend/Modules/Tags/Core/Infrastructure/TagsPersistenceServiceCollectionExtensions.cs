using System;
using Conduit.Tags.Core.Application;
using Conduit.Tags.Core.Domain;
using Conduit.Tags.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine.EntityFrameworkCore;

namespace Conduit.Tags.Core.Infrastructure;

public static class TagsPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddTagsPersistence(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        // Registers the context and wires Wolverine's outbox into it, so that domain events can be
        // staged in the same transaction as the tag change that raised them.
        services.AddDbContextWithWolverineIntegration<TagsDbContext>(configureDbContext);

        services.AddScoped<ITagsRepository, TagsRepository>();
        services.AddScoped<IArticleTagUsageRepository, ArticleTagUsageRepository>();
        services.AddScoped<ITagsReadRepository, TagsReadRepository>();
        services.AddScoped<IUnitOfWork, TagsUnitOfWork>();

        return services;
    }
}
