using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Conduit.UsersManagement.ApiEndpoints;
using Conduit.UsersManagement.ApiEndpoints.Models;
using Conduit.WebApi.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppSerializerContext.Default);
});
builder.Services.ConfigureUserManagementJsonOptions();


builder.Services
    .AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization()
    .AddConduitOpenApi()
    .AddProblemDetails();

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

app.MapDefaultEndpoints();

app.UseExceptionHandler()
    .UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapConduitOpenApi();
}

/* app
    .UseConduitPersistence()
    .UseConduitAuthentication()
    .UseConduitRouting()
    .UseConduitOpenApi()
    .UseConduitAuthorization()
    .UseConduitControllers()
    .UseConduitCors(); */

app.MapUserManagementEndpoints();

app.Run();

[JsonSerializable(typeof(ValidationProblemDetails))]
public partial class AppSerializerContext : JsonSerializerContext
{

}