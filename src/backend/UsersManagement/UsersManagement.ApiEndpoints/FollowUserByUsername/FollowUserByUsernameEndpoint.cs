using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.ApiEndpoints;
using Conduit.UsersManagement.ApiEndpoints.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Conduit.UsersManagement.ApiEndpoints.FollowUserByUsername;

public class FollowUserByUsernameEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/{username}/follow", HandleAsync)
            .WithSummary("Follow a user")
            .WithDescription("Follow a user by username<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints/#follow-user\">Conduit Spec for follow user endpoint</a>")
            .RequireAuthorization()
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static Task<Results<Ok<ProfileResponse>, UnprocessableEntity<ValidationProblemDetails>>> HandleAsync([FromRoute] string username, CancellationToken ct)
    {
        return Task.FromResult<Results<Ok<ProfileResponse>, UnprocessableEntity<ValidationProblemDetails>>>(
            TypedResults.Ok(
                new ProfileResponse()
                {
                    Profile = new()
                    {
                        Username = "Username",
                        Bio = "Bio",
                        Image = "Image",
                        Following = true
                    }
                }));
    }
}
