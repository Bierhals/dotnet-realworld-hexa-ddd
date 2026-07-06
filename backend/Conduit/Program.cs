using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Conduit;
using Conduit.Features.Articles;
using Conduit.Features.Comments;
using Conduit.Features.Favorites;
using Conduit.Features.Followers;
using Conduit.Features.Profiles;
using Conduit.Features.Tags;
using Conduit.Features.Users;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
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
}
else if (databaseProvider.ToLowerInvariant().Trim().Equals("postgresql", StringComparison.Ordinal))
{
    builder.AddNpgsqlDbContext<ConduitContext>(connectionName: "conduit-db");
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
    opt.SerializerOptions.DefaultIgnoreCondition = System
        .Text
        .Json
        .Serialization
        .JsonIgnoreCondition
        .WhenWritingNull
);

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

// The API runs behind a YARP gateway (see Conduit.AppHost) that forwards the
// public-facing scheme, host and path base via X-Forwarded-* headers. Trust
// those headers so that generated URLs (OpenAPI servers, Location headers,
// LinkGenerator, etc.) reflect the gateway's public address instead of the
// API's internal address.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto
        | ForwardedHeaders.XForwardedHost;
    // The gateway is not a fixed, known proxy address in the containerized
    // Aspire environment, so clear the default restrictions and trust all
    // proxies forwarding to this service.
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

// The gateway's route prefix ("/api" by default, see Conduit.AppHost) is sent
// on the X-Forwarded-Prefix header so the API can derive its path base
// dynamically instead of hard-coding it here too. This keeps the two in sync
// if the gateway route is ever reconfigured, and lets the API be reached
// without any path base at all (e.g. direct/local access) when the header is
// absent.
var defaultPathBase = new PathString("/api");
app.Use((context, next) =>
{
    var forwardedPrefix = context.Request.Headers["X-Forwarded-Prefix"].ToString();
    var pathBase = string.IsNullOrEmpty(forwardedPrefix)
        ? defaultPathBase
        : new PathString(forwardedPrefix);

    if (pathBase.HasValue && context.Request.Path.StartsWithSegments(pathBase, out var remainingPath))
    {
        context.Request.PathBase = context.Request.PathBase.Add(pathBase);
        context.Request.Path = remainingPath;
    }

    return next(context);
});

app.UseRouting();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.MapArticlesEndpoints();
app.MapCommentsEndpoints();
app.MapFavoritesEndpoints();
app.MapFollowersEndpoints();
app.MapProfilesEndpoints();
app.MapTagsEndpoints();
app.MapUsersEndpoints();

// Enable middleware to serve generated OpenAPI as a JSON endpoint
app.MapOpenApi("openapi/{documentName}.json");

// Enable middleware to serve openapi-ui assets(HTML, JS, CSS etc.)
app.MapScalarApiReference(
    "/api/api-docs",
    options => options.WithOperationTitleSource(OperationTitleSource.Path)
);

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope
        .ServiceProvider.GetRequiredService<ConduitContext>()
        .Database.EnsureCreated();
    // use context
}
app.Run();
