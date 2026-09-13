using System;
using Conduit.Articles.Application;
using Conduit.Articles.Domain;
using Conduit.Articles.Infrastructure.Persistence;
using Conduit.Articles.Infrastructure.Persistence.CommentNumbers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine.EntityFrameworkCore;

namespace Conduit.Articles.Infrastructure;

public static class ArticlesPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddArticlesPersistence(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        // Registers the context and wires Wolverine's outbox into it, so that domain events can be
        // staged in the same transaction as the article change that raised them.
        services.AddDbContextWithWolverineIntegration<ArticlesDbContext>(configureDbContext);

        services.AddScoped<IArticlesRepository, ArticlesRepository>();
        services.AddScoped<ICommentsRepository, CommentsRepository>();
        services.AddScoped<IArticleFavoritesRepository, ArticleFavoritesRepository>();
        services.AddScoped<IArticlesReadRepository, ArticlesReadRepository>();
        services.AddScoped<IUnitOfWork, ArticlesUnitOfWork>();

        // Which generator fits depends on whether the configured provider has sequences, which is
        // only known once the context is built.
        services.AddScoped<ICommentNumberGenerator>(sp =>
        {
            var dbContext = sp.GetRequiredService<ArticlesDbContext>();

            return ArticlesDbContext.SupportsSequences(dbContext.Database.ProviderName)
                ? new SequenceCommentNumberGenerator(dbContext)
                : new CounterTableCommentNumberGenerator(dbContext);
        });

        return services;
    }
}
