using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application.Commands.FollowUser;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Conduit.Shared.Infrastructure.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Identity.Api.Endpoints.Profiles;

internal sealed class FollowUserEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("{username}/follow", HandleAsync)
            .RequireAuthorization()
            .WithSummary("Follow a user")
            .WithDescription("Follow a user by username<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#follow-user\">Conduit Spec for follow user endpoint</a>")
            .Produces<ProfileEnvelope>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Results<Ok<ProfileEnvelope>, ProblemHttpResult>> HandleAsync(
        [Required]
        [Description("Username of the profile you want to follow")]
        string username,
        ICqrsMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new FollowUserCommand { Username = username }, cancellationToken);
        if (result.IsError)
        {
            return result.Errors.ToProblemResult();
        }

        return await ProfileEnvelopeFactory.BuildAsync(username, mediator, cancellationToken);
    }
}
