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
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services
    .AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization()
    .AddConduitOpenApiSetup()
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


// TODO: In Adapter verlagern, damit die internen Modelle nicht öffentlich gemacht werden müssen
[JsonSerializable(typeof(LoginUser))]
[JsonSerializable(typeof(LoginUserRequest))]
[JsonSerializable(typeof(UserResponse))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(NewUserRequest))]
[JsonSerializable(typeof(NewUser))]
[JsonSerializable(typeof(UpdateUser))]
[JsonSerializable(typeof(UpdateUserRequest))]
[JsonSerializable(typeof(ValidationProblemDetails))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
