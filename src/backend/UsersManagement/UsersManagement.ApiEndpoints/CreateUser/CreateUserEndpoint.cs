using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.ApiEndpoints;
using Conduit.UsersManagement.ApiEndpoints.Models;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Conduit.UsersManagement.ApiEndpoints.CreateUser;

internal sealed class CreateUserEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(string.Empty, HandleAsync)
            .WithSummary("Register a new user")
            .WithDescription("<a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints/#registration\">Conduit Spec for registration endpoint</a>");
    }

    private static Task<Results<Created<UserResponse>, UnprocessableEntity<ValidationProblemDetails>>> HandleAsync([FromBody] NewUserRequest request, CancellationToken ct)
    {
        return Task.FromResult<Results<Created<UserResponse>, UnprocessableEntity<ValidationProblemDetails>>>(
            TypedResults.Created((string?)null, new UserResponse()
            {
                User = new()
                {
                    Email = "test@test.de",
                    Token = "Token",
                    Username = "Username",
                    Bio = "Bio",
                    Image = "Image"
                }
            }));
    }
}
