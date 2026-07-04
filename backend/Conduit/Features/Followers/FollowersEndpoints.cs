using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure.Security;
using Conduit.Shared.RequestHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Features.Followers;

public static class FollowersEndpoints
{
    public static IEndpointRouteBuilder MapFollowersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var profiles = endpoints.MapGroup("/profiles")
            .WithTags("Profile")
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtIssuerOptions.Schemes });

        profiles.MapPost("{username}/follow", FollowUserAsync)
            .WithSummary("Follow a user")
            .WithDescription("Follow a user by username<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#follow-user\">Conduit Spec for follow user endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
        profiles.MapDelete("{username}/follow", UnfollowUserAsync)
            .WithSummary("Unfollow a user")
            .WithDescription("Unfollow a user by username<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#unfollow-user\">Conduit Spec for unfollow user endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }

    private static Task<Profiles.ProfileEnvelope> FollowUserAsync(
        ICommandHandler<Add.Command, Profiles.ProfileEnvelope> commandHandler,
        [Required] string username,
        CancellationToken cancellationToken
    ) => commandHandler.Handle(new Add.Command(username), cancellationToken);

    private static Task<Profiles.ProfileEnvelope> UnfollowUserAsync(
        ICommandHandler<Delete.Command, Profiles.ProfileEnvelope> commandHandler,
        [Required] string username,
        CancellationToken cancellationToken
    ) => commandHandler.Handle(new Delete.Command(username), cancellationToken);
}
