using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.ApiEndpoints;
using Conduit.UsersManagement.ApiEndpoints.GetCurrentUser;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.UsersManagement.ApiEndpoints.Users;

internal sealed class GetCurrentUserEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/user", HandleAsync)
            .WithSummary("Get current user")
            .WithDescription("Gets the currently logged-in user<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#get-current-user\">Conduit spec for Get Current User endpoint</a>")
            .WithTags("User and Authentication");
    }
    /* public static void MapGetUser(this Microsoft.AspNetCore.Routing.RouteGroupBuilder route)
    {
        Get("/api/user");
        Description(b => b
            .Produces(401)
            .ProducesProblemFE(422) //shortcut for .Produces<ErrorResponse>(422)
            .ProducesProblemFE<InternalErrorResponse>(500));
        Summary(s => {
            s.Summary = "Get current user";
            s.Description = "Gets the currently logged-in user<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#get-current-user\">Conduit spec for Get Current User endpoint</a>";
            s.Responses[422] = "Unprocessable Content";
        });
        AllowAnonymous();
    } */

    private static Task<Results<Ok<UserResponse>, UnauthorizedHttpResult, UnprocessableEntity<ValidationProblemDetails>, InternalServerError<ProblemDetails>>> HandleAsync(CancellationToken ct)
    {
        return Task.FromResult<Results<Ok<UserResponse>, UnauthorizedHttpResult, UnprocessableEntity<ValidationProblemDetails>, InternalServerError<ProblemDetails>>>(
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
