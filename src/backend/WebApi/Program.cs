using System.Text.Json.Serialization;
using Conduit.UsersManagement.ApiEndpoints;
using Conduit.UsersManagement.ApiEndpoints.GetCurrentUser;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddOpenApi();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

/* app
    .UseConduitPersistence()
    .UseConduitAuthentication()
    .UseConduitRouting()
    .UseConduitOpenApi()
    .UseConduitAuthorization()
    .UseConduitControllers()
    .UseConduitCors(); */

app.RegisterUserManagementEndpoints();

app.Run();

[JsonSerializable(typeof(UserResponse))]
[JsonSerializable(typeof(User))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}