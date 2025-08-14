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

namespace Conduit.UsersManagement.ApiEndpoints.Users;

internal sealed class GetCurrentUserEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/user", HandleAsync)
            .WithSummary("Get current user")
            .WithDescription("Gets the currently logged-in user<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#get-current-user\">Conduit spec for Get Current User endpoint</a>")
            .WithTags("User and Authentication")
            .RequireAuthorization();
    }

    private static Task<Results<Ok<UserResponse>, UnauthorizedHttpResult, UnprocessableEntity<ValidationProblemDetails>>> HandleAsync(CancellationToken ct)
    {
        return Task.FromResult<Results<Ok<UserResponse>, UnauthorizedHttpResult, UnprocessableEntity<ValidationProblemDetails>>>(
            TypedResults.Ok(new UserResponse()
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
