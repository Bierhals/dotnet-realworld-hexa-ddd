using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.Events;
using Conduit.Articles.Domain.ValueObjects;
using Conduit.Articles.Infrastructure.Persistence;
using Conduit.Shared.Application.EventHandling;
using Conduit.Shared.Infrastructure.Outbox;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Conduit.Shared.Infrastructure.IntegrationTests;

/// <summary>
/// Uses Articles as the concrete vehicle to exercise the shared outbox interceptor + processor
/// against a real relational transaction (SQLite, single open connection kept alive for the test's
/// lifetime so it behaves like a durable store, not a per-context throwaway).
/// </summary>
public sealed class OutboxIntegrationTests : IDisposable
{
    private static readonly DateTime PublishedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _connection = new("Filename=:memory:");

    public OutboxIntegrationTests() => _connection.Open();

    public void Dispose() => _connection.Dispose();

    [Fact]
    public async Task Publishing_an_article_writes_its_domain_event_as_an_outbox_row_in_the_same_transaction()
    {
        // Arrange
        using var context = CreateContext(CreateDispatcher(new RecordingHandler()));
        var article = APublishedArticle();
        var expectedEventId = article.DomainEvents.OfType<ArticlePublishedDomainEvent>().Single().Id;

        // Act
        context.Articles.Add(article);
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        var outboxMessage = context.Set<OutboxDomainEvent>().ToList().ShouldHaveSingleItem();
        outboxMessage.ProcessedOnUtc.ShouldBeNull();
        outboxMessage.DomainEventId.ShouldBe(expectedEventId);
        outboxMessage.DomainEventType.ShouldContain(nameof(ArticlePublishedDomainEvent));
        outboxMessage.HandlerType.ShouldBe(HandlerTypeName(typeof(RecordingHandler)));

        var deserializedEvent = (ArticlePublishedDomainEvent)JsonSerializer.Deserialize(
            outboxMessage.Payload, Type.GetType(outboxMessage.DomainEventType)!)!;
        deserializedEvent.ArticleId.ShouldBe(article.Id.Value);
        deserializedEvent.Author.ShouldBe("alice");
    }

    [Fact]
    public async Task Publishing_an_article_with_two_registered_handlers_writes_one_isolated_row_per_handler()
    {
        // Arrange
        var dispatcher = CreateDispatcher(new RecordingHandler(), new AnotherRecordingHandler());
        using var context = CreateContext(dispatcher);
        var article = APublishedArticle();
        var expectedEventId = article.DomainEvents.OfType<ArticlePublishedDomainEvent>().Single().Id;

        // Act
        context.Articles.Add(article);
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert - two independent rows, same event, one per handler, each with its own identity.
        var outboxMessages = context.Set<OutboxDomainEvent>().ToList();
        outboxMessages.Count.ShouldBe(2);
        outboxMessages.ShouldAllBe(m => m.DomainEventId == expectedEventId);
        outboxMessages.Select(m => m.Id).Distinct().Count().ShouldBe(2);
        outboxMessages.Select(m => m.HandlerType).ShouldBe(
            [HandlerTypeName(typeof(RecordingHandler)), HandlerTypeName(typeof(AnotherRecordingHandler))],
            ignoreOrder: true);
    }

    [Fact]
    public async Task Publishing_an_article_logs_the_domain_event_synchronously_when_the_outbox_row_is_written()
    {
        // Arrange - a mocked logger passed directly to the interceptor, with no OutboxProcessor
        // involved at all: the log call under test happens inside SaveChangesAsync itself. No
        // handler is registered either, proving the log doesn't depend on one existing.
        var loggerMock = new Mock<ILogger<DispatchDomainEventsInterceptor>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
        var options = new DbContextOptionsBuilder<ArticlesDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(new DispatchDomainEventsInterceptor(loggerMock.Object, CreateDispatcher()))
            .Options;
        using var context = new ArticlesDbContext(options);
        context.Database.EnsureCreated();
        var article = APublishedArticle();

        // Act
        context.Articles.Add(article);
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        // CA1873 doesn't apply here: this Verify expression asserts on ILogger.Log itself, not a
        // logging call site guarded by IsEnabled.
#pragma warning disable CA1873
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
#pragma warning restore CA1873

        context.Set<OutboxDomainEvent>().ShouldBeEmpty();
    }

    [Fact]
    public async Task A_row_left_unprocessed_by_a_crash_is_still_picked_up_once_the_processor_runs()
    {
        // Arrange - commit the aggregate change, then walk away: this simulates a crash between
        // commit and dispatch. The row is durably written but nothing has read it yet.
        Guid expectedEventId;
        using (var writeContext = CreateContext(CreateDispatcher(new RecordingHandler())))
        {
            var article = APublishedArticle();
            expectedEventId = article.DomainEvents.OfType<ArticlePublishedDomainEvent>().Single().Id;
            writeContext.Articles.Add(article);
            await writeContext.SaveChangesAsync(CancellationToken.None);
        }

        var dispatchCounts = new ConcurrentDictionary<Guid, int>();
        await using var provider = BuildProviderWithRecordingHandler(_connection, dispatchCounts);
        var processor = CreateProcessor(provider);

        // Act - drive a single poll cycle directly, instead of racing the background timer.
        await processor.ProcessBatchAsync(CancellationToken.None);

        // Assert
        dispatchCounts.ShouldContainKeyAndValue(expectedEventId, 1);

        using var readContext = CreateContext(CreateDispatcher());
        var outboxMessage = readContext.Set<OutboxDomainEvent>().Single();
        outboxMessage.ProcessedOnUtc.ShouldNotBeNull();
    }

    [Fact]
    public async Task When_one_of_two_handlers_fails_the_other_still_succeeds_and_the_failure_is_attributed_to_it()
    {
        // Arrange - two handlers for the same event; one always throws. Isolation means the
        // failure must neither block nor duplicate the other handler's delivery.
        var failure = new InvalidOperationException("Boom from RecordingHandler");
        using (var writeContext = CreateContext(
            CreateDispatcher(new RecordingHandler(throws: failure), new AnotherRecordingHandler())))
        {
            writeContext.Articles.Add(APublishedArticle());
            await writeContext.SaveChangesAsync(CancellationToken.None);
        }

        var services = new ServiceCollection();
        services.AddDbContext<ArticlesDbContext>((sp, options) =>
            options.UseSqlite(_connection).AddInterceptors(new DispatchDomainEventsInterceptor(
                NullLogger<DispatchDomainEventsInterceptor>.Instance, sp.GetRequiredService<DomainEventDispatcher>())));
        services.AddScoped<DomainEventDispatcher>();
        services.AddScoped<IDomainEventHandler<ArticlePublishedDomainEvent>>(
            _ => new RecordingHandler(throws: failure));
        services.AddScoped<IDomainEventHandler<ArticlePublishedDomainEvent>>(_ => new AnotherRecordingHandler());
        await using var provider = services.BuildServiceProvider();
        var processor = CreateProcessor(provider);

        // Act
        await processor.ProcessBatchAsync(CancellationToken.None);

        // Assert
        using var readContext = CreateContext(CreateDispatcher());
        var messages = readContext.Set<OutboxDomainEvent>().ToList();
        messages.Count.ShouldBe(2);

        var failedMessage = messages.Single(m => m.HandlerType == HandlerTypeName(typeof(RecordingHandler)));
        failedMessage.ProcessedOnUtc.ShouldBeNull();
        failedMessage.RetryCount.ShouldBe(1);
        failedMessage.LastError.ShouldBe(failure.Message);
        failedMessage.ClaimedBy.ShouldBeNull();

        var succeededMessage = messages.Single(m => m.HandlerType == HandlerTypeName(typeof(AnotherRecordingHandler)));
        succeededMessage.ProcessedOnUtc.ShouldNotBeNull();
        succeededMessage.LastError.ShouldBeNull();
    }

    [Fact]
    public async Task Two_processor_instances_racing_for_the_same_rows_never_dispatch_a_row_twice()
    {
        // Arrange - a uniquely named, shared-cache in-memory database so two independent
        // connections (standing in for two replicas) see the same rows, the way two replicas
        // would share one physical Postgres database in production.
        var connectionString =
            $"Data Source=outbox-concurrency-{Guid.NewGuid()};Mode=Memory;Cache=Shared;Default Timeout=5";
        using var keepAliveConnection = new SqliteConnection(connectionString);
        await keepAliveConnection.OpenAsync(TestContext.Current.CancellationToken);

        const int articleCount = 10;
        using (var seedContext = CreateContext(connectionString, CreateDispatcher(new RecordingHandler())))
        {
            for (var i = 0; i < articleCount; i++)
            {
                seedContext.Articles.Add(APublishedArticle());
            }

            await seedContext.SaveChangesAsync(CancellationToken.None);
        }

        var dispatchCounts = new ConcurrentDictionary<Guid, int>();
        await using var providerA = BuildProviderWithRecordingHandler(connectionString, dispatchCounts);
        await using var providerB = BuildProviderWithRecordingHandler(connectionString, dispatchCounts);
        var processorA = CreateProcessor(providerA);
        var processorB = CreateProcessor(providerB);

        // Act - drive both instances' poll cycle at the same time, racing for the same rows.
        await Task.WhenAll(
            processorA.ProcessBatchAsync(CancellationToken.None),
            processorB.ProcessBatchAsync(CancellationToken.None));

        // Assert - every row was handled, and none was handled more than once.
        dispatchCounts.Count.ShouldBe(articleCount);
        dispatchCounts.Values.ShouldAllBe(count => count == 1);
    }

    private static ServiceProvider BuildProviderWithRecordingHandler(
        SqliteConnection connection, ConcurrentDictionary<Guid, int> dispatchCounts) =>
        BuildProviderWithRecordingHandler(options => options.UseSqlite(connection), dispatchCounts);

    private static ServiceProvider BuildProviderWithRecordingHandler(
        string connectionString, ConcurrentDictionary<Guid, int> dispatchCounts) =>
        BuildProviderWithRecordingHandler(options => options.UseSqlite(connectionString), dispatchCounts);

    private static ServiceProvider BuildProviderWithRecordingHandler(
        Action<DbContextOptionsBuilder> configure, ConcurrentDictionary<Guid, int> dispatchCounts)
    {
        var services = new ServiceCollection();
        services.AddDbContext<ArticlesDbContext>((sp, options) =>
        {
            configure(options);
            options.AddInterceptors(new DispatchDomainEventsInterceptor(
                NullLogger<DispatchDomainEventsInterceptor>.Instance, sp.GetRequiredService<DomainEventDispatcher>()));
        });
        services.AddScoped<DomainEventDispatcher>();
        services.AddScoped<IDomainEventHandler<ArticlePublishedDomainEvent>>(
            _ => new RecordingHandler(dispatchCounts));

        return services.BuildServiceProvider();
    }

    private static OutboxProcessor<ArticlesDbContext> CreateProcessor(ServiceProvider provider) =>
        new(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new OutboxProcessorOptions()),
            NullLogger<OutboxProcessor<ArticlesDbContext>>.Instance);

    private ArticlesDbContext CreateContext(DomainEventDispatcher dispatcher) =>
        CreateContext(builder => builder.UseSqlite(_connection), dispatcher);

    private static ArticlesDbContext CreateContext(string connectionString, DomainEventDispatcher dispatcher) =>
        CreateContext(builder => builder.UseSqlite(connectionString), dispatcher);

    private static ArticlesDbContext CreateContext(
        Action<DbContextOptionsBuilder<ArticlesDbContext>> configure, DomainEventDispatcher dispatcher)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ArticlesDbContext>()
            .AddInterceptors(new DispatchDomainEventsInterceptor(
                NullLogger<DispatchDomainEventsInterceptor>.Instance, dispatcher));
        configure(optionsBuilder);

        var context = new ArticlesDbContext(optionsBuilder.Options);
        context.Database.EnsureCreated();

        return context;
    }

    /// <summary>
    /// A dispatcher backed by exactly the given handler instances - enough for
    /// DispatchDomainEventsInterceptor to fan a domain event out into one outbox row per handler.
    /// </summary>
    private static DomainEventDispatcher CreateDispatcher(
        params IDomainEventHandler<ArticlePublishedDomainEvent>[] handlers)
    {
        var services = new ServiceCollection();
        foreach (var handler in handlers)
        {
            services.AddSingleton(handler);
        }

        return new DomainEventDispatcher(services.BuildServiceProvider());
    }

    private static string HandlerTypeName(Type handlerType) =>
        $"{handlerType.FullName}, {handlerType.Assembly.GetName().Name}";

    private static Article APublishedArticle() =>
        Article.Publish(
            Username.Create("alice").Value,
            ArticleTitle.Create("How to train your dragon").Value,
            ArticleDescription.Create("Ever wonder how?").Value,
            ArticleBody.Create("You have to believe").Value,
            [],
            PublishedAt);

    private sealed class RecordingHandler(
        ConcurrentDictionary<Guid, int>? dispatchCounts = null, Exception? throws = null)
        : IDomainEventHandler<ArticlePublishedDomainEvent>
    {
        public Task Handle(ArticlePublishedDomainEvent domainEvent, CancellationToken ct)
        {
            // Task.FromException, not a synchronous throw: MethodInfo.Invoke (used by
            // DomainEventDispatcher) wraps a synchronous throw in a TargetInvocationException,
            // which isn't how a real async handler failing would surface its exception.
            if (throws is not null)
            {
                return Task.FromException(throws);
            }

            dispatchCounts?.AddOrUpdate(domainEvent.Id, 1, (_, count) => count + 1);

            return Task.CompletedTask;
        }
    }

    private sealed class AnotherRecordingHandler : IDomainEventHandler<ArticlePublishedDomainEvent>
    {
        public Task Handle(ArticlePublishedDomainEvent domainEvent, CancellationToken ct) => Task.CompletedTask;
    }
}
