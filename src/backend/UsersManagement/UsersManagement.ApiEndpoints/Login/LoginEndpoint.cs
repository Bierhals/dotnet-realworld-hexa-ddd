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

namespace Conduit.UsersManagement.ApiEndpoints.Login;

internal sealed class LoginEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/login", HandleAsync)
            .WithSummary("Existing user login")
            .WithDescription("Login for existing user<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#authentication\">Conduit Spec for login endpoint</a>")
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags("User and Authentication");
    }

    private static Task<Results<Ok<UserResponse>, UnprocessableEntity<ValidationProblemDetails>>> HandleAsync([FromBody] LoginUserRequest request, CancellationToken ct)
    {
        return Task.FromResult<Results<Ok<UserResponse>, UnprocessableEntity<ValidationProblemDetails>>>(
            TypedResults.Ok(new UserResponse()
            {
                User = new()
                {
                    Email = request.User.Email,
                    Token = "Token",
                    Username = "Username",
                    Bio = "Bio",
                    Image = "Image"
                }
            }));
    }
}