using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.ApiEndpoints;
using Conduit.UsersManagement.ApiEndpoints.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Conduit.UsersManagement.ApiEndpoints.UnfollowUserByUsername;

public class UnfollowUserByUsernameEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/{username}/unfollow", HandleAsync)
            .WithSummary("Unfollow a user")
            .WithDescription("Unfollow a user by username<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#unfollow-user\">Conduit Spec for unfollow user endpoint</a>")
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
