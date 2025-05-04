using Conduit.UsersManagement.ApiEndpoints.Users;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints(o =>
        o.Assemblies = new[]
        {
            typeof(GetCurrentUserEndpoint).Assembly
        })
    .SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.DocumentName = "Conduit.WebApi";
            s.Title = "Conduit.WebApi";
            s.Version = "v1";
        };
        o.ShortSchemaNames = true;
    });

/*builder.AddConduitConfiguration();

builder.Services
    .AddConduitAuthenticationSetup(builder.Configuration)
    .AddConduitServices()
    .AddConduitPersistence()
    .AddConduitMediatR()
    .AddConduitControllers()
    .AddConduitCors()
    .AddConduitOpenApiSetup(); */

var app = builder.Build();

app.UseFastEndpoints(c =>
    {
        c.Endpoints.ShortNames = true;
    })
    .UseSwaggerGen();

/* app
    .UseConduitPersistence()
    .UseConduitAuthentication()
    .UseConduitRouting()
    .UseConduitOpenApi()
    .UseConduitAuthorization()
    .UseConduitControllers()
    .UseConduitCors(); */

app.Run();
