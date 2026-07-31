using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Conduit.Host.WebApi;
using Conduit.Host.WebApi.Features.Articles;
using Conduit.Host.WebApi.Features.Comments;
using Conduit.Host.WebApi.Features.Favorites;
using Conduit.Host.WebApi.Features.Tags;
using Conduit.Host.WebApi.Infrastructure;
using Conduit.Host.WebApi.Infrastructure.Errors;
using Conduit.Identity.Api;
using Conduit.Identity.Infrastructure;
using Conduit.Identity.Infrastructure.Persistence;
using Conduit.Shared.Application.Optional;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var defaultDatabaseConnectionString = "Filename=realworld.db";
var defaultDatabaseProvider = "sqlite";

var builder = WebApplication.CreateBuilder(args);

// Add common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
builder.AddServiceDefaults();

// take the connection string from the environment variable or use hard-coded database name
var connectionString = defaultDatabaseConnectionString;

// take the database provider from the environment variable or use hard-coded database provider
var databaseProvider = Environment.GetEnvironmentVariable("DATABASE_PROVIDER") ?? defaultDatabaseProvider;

if (databaseProvider.ToLowerInvariant().Trim().Equals("sqlite", StringComparison.Ordinal))
{
    builder.Services.AddDbContext<ConduitContext>(options =>
    {
        options.UseSqlite(connectionString);
    });
    builder.Services.AddIdentityModule(options => options.UseSqlite(connectionString));
}
else if (databaseProvider.ToLowerInvariant().Trim().Equals("postgresql", StringComparison.Ordinal))
{
    builder.AddNpgsqlDbContext<ConduitContext>(connectionName: "conduit-db");
    builder.Services.AddIdentityModule(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("conduit-db")));
}
else
{
    throw new InvalidOperationException(
        "Database provider unknown. Please check configuration"
    );
}

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
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.MapArticlesEndpoints();
app.MapCommentsEndpoints();
app.MapFavoritesEndpoints();
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
    // ConduitContext and IdentityDbContext can share one physical database. EnsureCreated()
    // and IRelationalDatabaseCreator.HasTables() only check whether *any* table exists in that
    // database, not whether this context's own tables do, so they can't reliably decide whether
    // Identity's tables still need to be created. Create them directly instead, and treat
    // "already exists" as success (they were created by a previous run of this same host).
    scope.ServiceProvider.GetRequiredService<ConduitContext>().Database.EnsureCreated();

    var identityDatabaseCreator = scope
        .ServiceProvider.GetRequiredService<IdentityDbContext>()
        .GetService<IRelationalDatabaseCreator>();
    try
    {
        identityDatabaseCreator.CreateTables();
    }
    catch (DbException)
    {
        // Identity's tables already exist from a previous run.
    }
}
app.Run();
