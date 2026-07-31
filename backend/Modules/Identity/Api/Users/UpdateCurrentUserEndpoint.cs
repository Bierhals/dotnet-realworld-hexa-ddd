using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application.Commands.UpdateUser;
using Conduit.Identity.Application.Queries.CurrentUser;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Conduit.Shared.Infrastructure.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Identity.Api.Endpoints.Users;

internal sealed class UpdateCurrentUserEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("", HandleAsync)
            .WithSummary("Update current user")
            .WithDescription("Updated user information for current user<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#update-user\">Conduit Spec for update user endpoint</a>")
            .Produces<UserEnvelope>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Results<Ok<UserEnvelope>, ProblemHttpResult>> HandleAsync(
        [Description("User details to update. At least one field is required.")]
        UpdateUserRequest request,
        ICqrsMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand
        {
            Username = request.User.Username,
            Email = request.User.Email,
            Password = request.User.Password,
            Bio = request.User.Bio,
            Image = request.User.Image,
        };

        var result = await mediator.Send(command, cancellationToken);
        if (result.IsError)
        {
            return result.Errors.ToProblemResult();
        }

        var currentUser = await mediator.Send(new CurrentUserQuery(), cancellationToken);
        if (currentUser.IsError)
        {
            return currentUser.Errors.ToProblemResult();
        }

        return TypedResults.Ok(UserEnvelopeFactory.Create(currentUser.Value));
    }
}
