using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application.Commands.AuthenticateUser;
using Conduit.Identity.Application.Queries.CurrentUser;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Conduit.Shared.Infrastructure.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Identity.Api.Endpoints.Users;

internal sealed class LoginUserEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("login", HandleAsync)
            .WithSummary("Existing user login")
            .WithDescription("Login for existing user<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#authentication\">Conduit Spec for login endpoint</a>")
            .Produces<UserEnvelope>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Results<Ok<UserEnvelope>, ProblemHttpResult>> HandleAsync(
        [Description("Credentials to use")]
        LoginUserRequest request,
        ICqrsMediator mediator,
        ICurrentUserSetter currentUserSetter,
        CancellationToken cancellationToken)
    {
        var command = new AuthenticateUserCommand
        {
            Email = request.User.Email,
            Password = request.User.Password,
        };

        var result = await mediator.Send(command, cancellationToken);
        if (result.IsError)
        {
            return result.Errors.ToProblemResult();
        }

        currentUserSetter.SetCurrentUsername(result.Value);
        var currentUser = await mediator.Send(new CurrentUserQuery(), cancellationToken);
        if (currentUser.IsError)
        {
            return currentUser.Errors.ToProblemResult();
        }

        return TypedResults.Ok(UserEnvelopeFactory.Create(currentUser.Value));
    }
}
