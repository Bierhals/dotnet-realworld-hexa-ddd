using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.ApiEndpoints;
using Conduit.UsersManagement.ApiEndpoints.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Conduit.UsersManagement.ApiEndpoints.GetProfileByUsername;

public class GetProfileByUsernameEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/{username}", HandleAsync)
            .WithSummary("Get a profile")
            .WithDescription("Get a profile of a user of the system. Auth is optional<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints/#get-profile\">Conduit spec for Get Profile</a>")
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
