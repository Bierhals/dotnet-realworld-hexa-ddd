using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;
using Conduit.Articles.Infrastructure;
using Conduit.Articles.Infrastructure.Persistence;
using Conduit.Tags.Core.Application;
using Conduit.Tags.Core.Infrastructure;
using Conduit.Tags.Core.Infrastructure.Persistence;
using JasperFx.CodeGeneration.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shouldly;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.Sqlite;
using Wolverine.Tracking;
using ArticlesUnitOfWork = Conduit.Articles.Application.IUnitOfWork;

namespace Conduit.Shared.Infrastructure.IntegrationTests;

/// <summary>
/// Covers the whole path that keeps the tag catalog in step with the articles: the Articles module
/// saves an article, its domain event is translated into an integration event, and the Tags
/// module applies it to its catalog. Both modules run for real, over one database like in the host.
/// </summary>
[Collection(WolverineHostCollection.Name)]
public sealed class TagCatalogSyncTests : IAsyncLifetime
{
    private static readonly Username Author = Username.Rehydrate("the-author");

    private readonly string _databasePath =
        Path.Combine(Path.GetTempPath(), $"conduit-tag-sync-{Guid.NewGuid():N}.db");

    private IHost _host = null!;

    private string ConnectionString => $"Data Source={_databasePath}";

    public async ValueTask InitializeAsync()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);

        builder.Services.AddArticlesPersistence(options => options.UseSqlite(ConnectionString));
        builder.Services.AddTagsPersistence(options => options.UseSqlite(ConnectionString));
        builder.Services.AddTagsApplication();

        // Mirrors the host's Wolverine setup - the settings that decide whether a handler can be
        // compiled and how it is wrapped are exactly what this test is about.
        builder.UseWolverine(options =>
        {
            options.PersistMessagesWithSqlite(ConnectionString);
            options.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;
            options.Durability.MessageIdentity = MessageIdentity.IdAndDestination;
            options.ServiceLocationPolicy = ServiceLocationPolicy.AllowedButWarn;
            options.Policies.AutoApplyTransactions();
            options.Policies.UseDurableLocalQueues();

            options.OnAnyException()
                .RetryWithCooldown(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

            options.AddArticlesMessaging(useRabbitMq: false)
                .AddTagsMessaging(useRabbitMq: false);
        });

        _host = builder.Build();

        // Wolverine does not support in-memory SQLite for durable messaging, so the store has to
        // be a real file, and its tables have to exist before Wolverine starts.
        using (var scope = _host.Services.CreateScope())
        {
            CreateModuleTables(scope.ServiceProvider.GetRequiredService<ArticlesDbContext>());
            CreateModuleTables(scope.ServiceProvider.GetRequiredService<TagsDbContext>());
        }

        await _host.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _host.StopAsync();
        _host.Dispose();

        File.Delete(_databasePath);
    }

    [Fact]
    public async Task A_published_article_adds_its_tags_to_the_catalog()
    {
        await PublishAsync("Dragons", "dragons", "training");

        (await CatalogAsync()).ShouldBe(new Dictionary<string, int> { ["dragons"] = 1, ["training"] = 1 });
    }

    [Fact]
    public async Task Changing_the_tags_of_an_article_references_the_new_ones_and_releases_the_old_ones()
    {
        await PublishAsync("Dragons", "dragons", "training");

        await EditTagsAsync("dragons", "dragons", "flying");

        (await CatalogAsync()).ShouldBe(new Dictionary<string, int> { ["dragons"] = 1, ["flying"] = 1 });
    }

    [Fact]
    public async Task A_deleted_article_gives_up_all_its_tags()
    {
        await PublishAsync("Dragons", "dragons", "training");

        await DeleteAsync("dragons");

        (await CatalogAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task A_tag_stays_in_the_catalog_as_long_as_another_article_uses_it()
    {
        await PublishAsync("Dragons", "dragons", "training");
        await PublishAsync("More dragons", "dragons");

        await DeleteAsync("dragons");

        (await CatalogAsync()).ShouldBe(new Dictionary<string, int> { ["dragons"] = 1 });

        await DeleteAsync("more-dragons");

        (await CatalogAsync()).ShouldBeEmpty();
    }

    private Task PublishAsync(string title, params string[] tagNames) =>
        ChangeArticlesAsync(async (articles, unitOfWork) =>
        {
            articles.Add(Article.Publish(
                Author,
                ArticleTitle.Rehydrate(title),
                ArticleDescription.Rehydrate("a description"),
                ArticleBody.Rehydrate("a body"),
                [.. tagNames.Select(name => TagName.Create(name).Value)],
                DateTime.UtcNow));

            await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        });

    private Task EditTagsAsync(string slug, params string[] tagNames) =>
        ChangeArticlesAsync(async (articles, unitOfWork) =>
        {
            var article = await FindAsync(articles, slug);

            article.Edit(
                Author,
                title: null,
                description: null,
                body: null,
                [.. tagNames.Select(name => TagName.Create(name).Value)],
                DateTime.UtcNow).IsError.ShouldBeFalse();

            await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        });

    private Task DeleteAsync(string slug) =>
        ChangeArticlesAsync(async (articles, unitOfWork) =>
        {
            var article = await FindAsync(articles, slug);

            article.Delete(Author).IsError.ShouldBeFalse();
            articles.Remove(article);

            await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        });

    private static async Task<Article> FindAsync(IArticlesRepository articles, string slug) =>
        await articles.GetBySlugAsync(ArticleSlug.Rehydrate(slug), TestContext.Current.CancellationToken)
        ?? throw new InvalidOperationException($"No article '{slug}'.");

    /// <summary>
    /// Runs a change in a scope of its own, like a request would, and returns once every message it
    /// caused - across both modules - has been handled. A handler that fails fails the test.
    /// </summary>
    private async Task ChangeArticlesAsync(Func<IArticlesRepository, ArticlesUnitOfWork, Task> change) =>
        await _host.TrackActivity().ExecuteAndWaitAsync((Func<IMessageContext, Task>)(async _ =>
        {
            using var scope = _host.Services.CreateScope();

            await change(
                scope.ServiceProvider.GetRequiredService<IArticlesRepository>(),
                scope.ServiceProvider.GetRequiredService<ArticlesUnitOfWork>());
        }));

    private async Task<Dictionary<string, int>> CatalogAsync()
    {
        using var scope = _host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TagsDbContext>();

        var tags = await context.Tags.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken);

        return tags.ToDictionary(tag => tag.Id.Value, tag => tag.ReferenceCount);
    }

    // The same guard the host uses: whichever module runs first creates the database, and each one
    // creates its own tables unless they exist already.
    private static void CreateModuleTables(DbContext moduleDbContext)
    {
        var creator = moduleDbContext.GetService<IRelationalDatabaseCreator>();

        if (!creator.Exists())
        {
            creator.Create();
        }

        try
        {
            creator.CreateTables();
        }
        catch (DbException)
        {
            // The module's tables already exist.
        }
    }
}
