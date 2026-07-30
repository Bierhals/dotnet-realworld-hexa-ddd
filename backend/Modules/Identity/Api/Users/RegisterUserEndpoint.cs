using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application.Commands.RegisterUser;
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

internal sealed class RegisterUserEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", HandleAsync)
            .WithSummary("Register a new user")
            .WithDescription("Register a new user<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#registration\">Conduit Spec for registration endpoint</a>")
            .Produces<UserEnvelope>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Results<Created<UserEnvelope>, ProblemHttpResult>> HandleAsync(
        [Description("Details of the new user to register")]
        RegisterUserRequest request,
        ICqrsMediator mediator,
        ICurrentUserSetter currentUserSetter,
        LinkGenerator linkGenerator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand
        {
            Username = request.User.Username,
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

        var location = linkGenerator.GetPathByName(httpContext, GetCurrentUserEndpoint.Name, null);
        return TypedResults.Created(location, UserEnvelopeFactory.Create(currentUser.Value));
    }
}
