using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.Events;
using Conduit.Articles.Domain.ValueObjects;
using Conduit.Articles.Infrastructure.Persistence;
using Conduit.Shared.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shouldly;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Sqlite;
using Wolverine.Tracking;

namespace Conduit.Shared.Infrastructure.IntegrationTests;

/// <summary>
/// Covers the path every module takes when it saves an aggregate: the domain events it raised are
/// committed together with the change and then delivered to their handlers out of band.
/// </summary>
public sealed class DomainEventPublishingTests : IAsyncLifetime
{
    // Wolverine does not support in-memory SQLite for durable messaging, so the store has to be a
    // real file.
    private readonly string _databasePath =
        Path.Combine(Path.GetTempPath(), $"conduit-tests-{Guid.NewGuid():N}.db");

    private IHost _host = null!;

    private string ConnectionString => $"Data Source={_databasePath}";

    public async ValueTask InitializeAsync()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);

        builder.Services.AddDbContextWithWolverineIntegration<ArticlesDbContext>(
            options => options.UseSqlite(ConnectionString));

        builder.UseWolverine(options =>
        {
            options.PersistMessagesWithSqlite(ConnectionString);
            options.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;
            options.Durability.MessageIdentity = MessageIdentity.IdAndDestination;
            options.Policies.AutoApplyTransactions();
            options.Policies.UseDurableLocalQueues();

            // Mirrors the host's retry policy, with no cooldown so the tests stay fast.
            options.OnAnyException()
                .RetryWithCooldown(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

            // Only this assembly's handlers - the module's own handlers would add noise the
            // assertions below don't care about.
            options.Discovery.IncludeAssembly(typeof(DomainEventPublishingTests).Assembly);
        });

        _host = builder.Build();

        // The schema has to exist before Wolverine starts: it provisions its own tables on startup
        // and EnsureCreated skips a database that already has any table in it.
        using (var scope = _host.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<ArticlesDbContext>().Database.EnsureCreatedAsync();
        }

        await _host.StartAsync();

        RecordingArticlePublishedHandler.Reset();
    }

    public async ValueTask DisposeAsync()
    {
        await _host.StopAsync();
        _host.Dispose();

        File.Delete(_databasePath);
    }

    [Fact]
    public async Task Saving_an_aggregate_delivers_the_domain_event_it_raised_to_a_handler()
    {
        // Arrange
        var article = ArticleBuilder.Published("A durable article");
        var raisedEvent = (ArticlePublishedDomainEvent)article.DomainEvents[0];

        // Act
        await _host.TrackActivity().ExecuteAndWaitAsync(_ => SaveAsync(article));

        // Assert
        var handled = RecordingArticlePublishedHandler.Handled.ShouldHaveSingleItem();
        handled.ArticleId.ShouldBe(article.Id.Value);

        // The event survives the round trip through the outbox unchanged. Without the init
        // accessors on DomainEvent these would be a fresh Guid and "whenever it was read".
        handled.Id.ShouldBe(raisedEvent.Id);
        handled.OccurredOnUtc.ShouldBe(raisedEvent.OccurredOnUtc);
    }

    [Fact]
    public async Task An_aggregate_forgets_its_domain_events_once_they_have_been_published()
    {
        // Arrange
        var article = ArticleBuilder.Published("Published exactly once");
        await _host.TrackActivity().ExecuteAndWaitAsync(_ => SaveAsync(article));

        // Act - save the very same instance a second time. Had the publishing path not cleared
        // its events, they would go out again here.
        await _host.TrackActivity().ExecuteAndWaitAsync(_ => UpdateAsync(article));

        // Assert
        RecordingArticlePublishedHandler.Handled.Count.ShouldBe(1);
    }

    [Fact]
    public async Task A_save_that_fails_delivers_no_domain_event()
    {
        // Arrange - make the write fail for a reason the aggregate knows nothing about. Dropping
        // the table is blunt, but it is the failure the guarantee is about: the change did not
        // reach the database, so the event it raised must not reach a handler either.
        using (var scope = _host.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ArticlesDbContext>();
            await context.Database.ExecuteSqlRawAsync(
                "DROP TABLE Articles", TestContext.Current.CancellationToken);
        }

        var article = ArticleBuilder.Published("Never written");

        // Act
        var save = async () => await SaveAsync(article);

        // Assert - no partial outcome
        await save.ShouldThrowAsync<DbUpdateException>();
        RecordingArticlePublishedHandler.Handled.ShouldBeEmpty();
    }

    [Fact]
    public async Task A_failing_handler_leaves_the_aggregate_change_committed_and_is_retried()
    {
        // Arrange
        RecordingArticlePublishedHandler.FailUntilAttempt = 2;
        var article = ArticleBuilder.Published("Retried but committed");

        // Act
        await _host.TrackActivity()
            .DoNotAssertOnExceptionsDetected()
            .ExecuteAndWaitAsync(_ => SaveAsync(article));

        // Assert - the handler failed once and ran again, while the article stayed committed
        RecordingArticlePublishedHandler.Attempts.ShouldBeGreaterThanOrEqualTo(2);
        RecordingArticlePublishedHandler.Handled.ShouldHaveSingleItem();

        using var scope = _host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ArticlesDbContext>();
        (await context.Articles.AnyAsync(a => a.Id == article.Id, TestContext.Current.CancellationToken))
            .ShouldBeTrue();
    }

    private async Task SaveAsync(Article article)
    {
        using var scope = _host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ArticlesDbContext>();
        context.Articles.Add(article);

        await PublishAsync(scope, context);
    }

    private async Task UpdateAsync(Article article)
    {
        using var scope = _host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ArticlesDbContext>();
        context.Articles.Update(article);

        await PublishAsync(scope, context);
    }

    private static Task PublishAsync(IServiceScope scope, ArticlesDbContext context)
    {
        var outbox = scope.ServiceProvider.GetRequiredService<IDbContextOutbox<ArticlesDbContext>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DomainEventPublishingTests>>();

        return outbox.SaveChangesAndPublishDomainEventsAsync(
            context, logger, TestContext.Current.CancellationToken);
    }
}

internal static class ArticleBuilder
{
    public static Article Published(string title) =>
        Article.Publish(
            Username.Rehydrate("the-author"),
            ArticleTitle.Rehydrate(title),
            ArticleDescription.Rehydrate("a description"),
            ArticleBody.Rehydrate("a body"),
            [],
            DateTime.UtcNow);
}

/// <summary>
/// Records what it was handed. A fake rather than a mock: the assertions are about which events
/// arrived, not about how the handler was called.
/// </summary>
public sealed class RecordingArticlePublishedHandler
{
    private static int _attempts;

    public static List<ArticlePublishedDomainEvent> Handled { get; } = [];

    public static int Attempts => _attempts;

    /// <summary>Number of the first attempt that should succeed; 0 means "never fail".</summary>
    public static int FailUntilAttempt { get; set; }

    public static void Reset()
    {
        Handled.Clear();
        _attempts = 0;
        FailUntilAttempt = 0;
    }

    public void Handle(ArticlePublishedDomainEvent domainEvent)
    {
        var attempt = Interlocked.Increment(ref _attempts);

        if (attempt < FailUntilAttempt)
        {
            throw new InvalidOperationException($"Deliberate failure on attempt {attempt}.");
        }

        lock (Handled)
        {
            Handled.Add(domainEvent);
        }
    }
}
