using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Conduit.Articles.Api;
using Conduit.Articles.Application;
using Conduit.Articles.Infrastructure;
using Conduit.Articles.Infrastructure.Persistence;
using Conduit.Host.WebApi;
using Conduit.Identity.Api;
using Conduit.Identity.Application;
using Conduit.Identity.Infrastructure;
using Conduit.Identity.Infrastructure.Persistence;
using Conduit.Shared.Application.Optional;
using Conduit.Shared.Infrastructure.Messaging;
using Conduit.Tags.Core.Api;
using Conduit.Tags.Core.Application;
using Conduit.Tags.Core.Infrastructure;
using Conduit.Tags.Core.Infrastructure.Persistence;
using JasperFx.CodeGeneration.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;
using Wolverine.Sqlite;

var defaultDatabaseConnectionString = "Filename=realworld.db";
var defaultDatabaseProvider = "sqlite";

var builder = WebApplication.CreateBuilder(args);

// Add common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
builder.AddServiceDefaults();

// take the connection string from the environment variable or use hard-coded database name
var connectionString = defaultDatabaseConnectionString;

// take the database provider from the environment variable or use hard-coded database provider
var databaseProvider = Environment.GetEnvironmentVariable("DATABASE_PROVIDER") ?? defaultDatabaseProvider;

builder.Services.AddIdentityApplication();
builder.Services.AddTagsApplication();
builder.Services.AddArticlesApplication();

var usePostgres = databaseProvider.ToLowerInvariant().Trim().Equals("postgresql", StringComparison.Ordinal);

if (usePostgres)
{
    connectionString = builder.Configuration.GetConnectionString("conduit-db")
        ?? throw new InvalidOperationException("Connection string 'conduit-db' is not configured.");

    builder.Services.AddIdentityPersistence(options => options.UseNpgsql(connectionString));
    builder.Services.AddTagsPersistence(options => options.UseNpgsql(connectionString));
    builder.Services.AddArticlesPersistence(options => options.UseNpgsql(connectionString));
}
else if (databaseProvider.ToLowerInvariant().Trim().Equals("sqlite", StringComparison.Ordinal))
{
    builder.Services.AddIdentityPersistence(options => options.UseSqlite(connectionString));
    builder.Services.AddTagsPersistence(options => options.UseSqlite(connectionString));
    builder.Services.AddArticlesPersistence(options => options.UseSqlite(connectionString));
}
else
{
    throw new InvalidOperationException(
        "Database provider unknown. Please check configuration"
    );
}

// Provider-independent, unlike the persistence registrations above.
builder.Services.AddIdentitySecurity();
builder.Services.AddArticlesAdapters();

var rabbitMqConnectionString = builder.Configuration.GetConnectionString("rabbitmq");
var useRabbitMq = !string.IsNullOrWhiteSpace(rabbitMqConnectionString);

builder.UseWolverine(options =>
{
    if (usePostgres)
    {
        options.PersistMessagesWithPostgresql(connectionString, "wolverine");
    }
    else
    {
        options.PersistMessagesWithSqlite(connectionString);
    }

    options.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;

    // The modules keep their adapters internal, which Wolverine's generated handler code cannot
    // construct directly. Those dependencies are resolved from the scoped container instead.
    options.ServiceLocationPolicy = ServiceLocationPolicy.AllowedButWarn;
    options.Durability.MessageIdentity = MessageIdentity.IdAndDestination;
    options.Policies.AutoApplyTransactions();
    options.Policies.UseDurableLocalQueues();

    options.OnAnyException()
        .RetryWithCooldown(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));

    if (useRabbitMq)
    {
        options.UseRabbitMqUsingNamedConnection("rabbitmq")
            .UseQuorumQueues()
            .AutoProvision()
            .EnableWolverineControlQueues();

        options.Policies.UseDurableOutboxOnAllSendingEndpoints();
    }

    options.AddIdentityMessaging(useRabbitMq)
        .AddTagsMessaging(useRabbitMq)
        .AddArticlesMessaging(useRabbitMq);
});

builder.Services.AddLocalization(x => x.ResourcesPath = "Resources");
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.DefaultIgnoreCondition = System
        .Text
        .Json
        .Serialization
        .JsonIgnoreCondition
        .WhenWritingNull;
    opt.SerializerOptions.Converters.Add(new OptionalJsonConverterFactory());
});

builder.Services.AddOpenApi(options =>
{
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        var endpointMetadata = context.Description.ActionDescriptor.EndpointMetadata;

        if (endpointMetadata.OfType<IAllowAnonymous>().Any())
        {
            return Task.CompletedTask;
        }

        if (endpointMetadata.OfType<IAuthorizeData>().Any())
        {
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
            });
        }

        return Task.CompletedTask;
    });

    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "RealWorld Conduit API",
            Version = "2.0.0",
            Description = "Conduit API documentation",
            Contact = new()
            {
                Name = "RealWorld",
                Url = new Uri("https://realworld-docs.netlify.app/")
            },
            License = new()
            {
                Name = "MIT",
                Url = new Uri("https://opensource.org/licenses/MIT")
            },
        };
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer", // "bearer" refers to the header name here
                In = ParameterLocation.Header,
                BearerFormat = "JWT",
                Description = "Please insert JWT with Bearer into field",
                Name = "Authorization"
            }
        };

        return Task.CompletedTask;
    });
    // schema names that include the full namespace of the model
    options.CreateSchemaReferenceId = (type) =>
    {
        var schemaRefId = OpenApiOptions.CreateDefaultSchemaReferenceId(type);
        // Ignore primitive types
        if (schemaRefId is null)
        {
            return null;
        }

        // Replace '+' with '.' to handle nested types
        return type.Type.FullName!.Replace("+", ".", StringComparison.Ordinal);
    };
});

builder.Services.AddCors();

builder.Services.AddConduit();

builder.Services.AddJwt();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseRouting();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

app.MapArticlesEndpoints();
app.MapTagsEndpoints();
app.MapIdentityEndpoints();

// Enable middleware to serve generated OpenAPI as a JSON endpoint
app.MapOpenApi("openapi/{documentName}.json");

// Enable middleware to serve openapi-ui assets(HTML, JS, CSS etc.)
app.MapScalarApiReference(
    "api-docs",
    options => options.WithOperationTitleSource(OperationTitleSource.Path)
);

using (var scope = app.Services.CreateScope())
{
    // The module contexts can share one physical database, so each of them creates its own tables
    // behind a guard of its own - see CreateModuleTables.
    CreateModuleTables(scope.ServiceProvider.GetRequiredService<IdentityDbContext>());
    CreateModuleTables(scope.ServiceProvider.GetRequiredService<TagsDbContext>());
    CreateModuleTables(scope.ServiceProvider.GetRequiredService<ArticlesDbContext>());

    ArticlesInfrastructureInitializer.EnsureCommentNumbersReady(
        scope.ServiceProvider.GetRequiredService<ArticlesDbContext>());
}

static void CreateModuleTables(DbContext moduleDbContext)
{
    var creator = moduleDbContext.GetService<IRelationalDatabaseCreator>();

    // Whichever module runs first has to bring the physical database into existence.
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
        // The module's tables already exist from a previous run.
    }
}

app.Run();
