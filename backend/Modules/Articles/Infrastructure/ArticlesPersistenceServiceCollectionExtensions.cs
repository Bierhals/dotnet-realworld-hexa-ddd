using System;
using Conduit.Articles.Application;
using Conduit.Articles.Domain;
using Conduit.Articles.Infrastructure.Persistence;
using Conduit.Articles.Infrastructure.Persistence.CommentNumbers;
using Conduit.Shared.Application.EventHandling;
using Conduit.Shared.Infrastructure;
using Conduit.Shared.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Conduit.Articles.Infrastructure;

public static class ArticlesPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddArticlesPersistence(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        services.AddDbContext<ArticlesDbContext>((sp, options) =>
        {
            configureDbContext(options);
            options.AddInterceptors(new DispatchDomainEventsInterceptor(
                sp.GetRequiredService<ILogger<DispatchDomainEventsInterceptor>>(),
                sp.GetRequiredService<DomainEventDispatcher>()));
        });

        services.AddHostedService<OutboxProcessor<ArticlesDbContext>>();

        services.AddScoped<IArticlesRepository, ArticlesRepository>();
        services.AddScoped<ICommentsRepository, CommentsRepository>();
        services.AddScoped<IArticleFavoritesRepository, ArticleFavoritesRepository>();
        services.AddScoped<IArticlesReadRepository, ArticlesReadRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ArticlesDbContext>());

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
