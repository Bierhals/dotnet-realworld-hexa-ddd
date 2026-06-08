using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Conduit;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

// read database configuration (database provider + database connection) from environment variables
//Environment.GetEnvironmentVariable(DEFAULT_DATABASE_PROVIDER)
//Environment.GetEnvironmentVariable(DEFAULT_DATABASE_CONNECTION_STRING)
var defaultDatabaseConnectionString = "Filename=realworld.db";
var defaultDatabaseProvider = "sqlite";

var builder = WebApplication.CreateBuilder(args);

// take the connection string from the environment variable or use hard-coded database name
var connectionString = defaultDatabaseConnectionString;

// take the database provider from the environment variable or use hard-coded database provider
var databaseProvider = defaultDatabaseProvider;

builder.Services.AddDbContext<ConduitContext>(options =>
{
    if (databaseProvider.ToLowerInvariant().Trim().Equals("sqlite", StringComparison.Ordinal))
    {
        options.UseSqlite(connectionString);
    }
    else if (
        databaseProvider.ToLowerInvariant().Trim().Equals("sqlserver", StringComparison.Ordinal)
    )
    {
        // only works in windows container
        options.UseSqlServer(connectionString);
    }
    else
    {
        throw new InvalidOperationException(
            "Database provider unknown. Please check configuration"
        );
    }
});

builder.Services.AddLocalization(x => x.ResourcesPath = "Resources");

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "RealWorld API",
            Version = "v1"
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

        // Apply it as a requirement for all operations
        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations!))
        {
            operation.Value.Security ??= [];
            operation.Value.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        }
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
builder
    .Services.AddMvc(opt =>
    {
        opt.Filters.Add<ValidatorActionFilter>();
        opt.EnableEndpointRouting = false;
    })
    .AddJsonOptions(opt =>
        opt.JsonSerializerOptions.DefaultIgnoreCondition = System
            .Text
            .Json
            .Serialization
            .JsonIgnoreCondition
            .WhenWritingNull
    );

builder.Services.AddConduit();

builder.Services.AddJwt();

var app = builder.Build();
app.UsePathBase("/api");

app.Services.GetRequiredService<ILoggerFactory>().AddSerilogLogging();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseAuthentication();
app.UseMvc();

// Enable middleware to serve generated OpenAPI as a JSON endpoint
app.MapOpenApi("openapi/{documentName}.json");

// Enable middleware to serve openapi-ui assets(HTML, JS, CSS etc.)
app.MapScalarApiReference("/api/api-docs");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope
        .ServiceProvider.GetRequiredService<ConduitContext>()
        .Database.EnsureCreated();
    // use context
}
app.Run();
