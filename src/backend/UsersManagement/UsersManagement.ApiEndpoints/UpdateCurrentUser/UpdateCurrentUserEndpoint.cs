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

namespace Conduit.UsersManagement.ApiEndpoints.UpdateCurrentUser;

internal sealed class UpdateCurrentUserEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(string.Empty, HandleAsync)
            .WithSummary("Update current user")
            .WithDescription("Updated user information for current user<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#update-user\">Conduit spec for Update User</a>")
            .RequireAuthorization()
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static Task<Results<Ok<UserResponse>, UnprocessableEntity<ValidationProblemDetails>>> HandleAsync([FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        return Task.FromResult<Results<Ok<UserResponse>, UnprocessableEntity<ValidationProblemDetails>>>(
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
