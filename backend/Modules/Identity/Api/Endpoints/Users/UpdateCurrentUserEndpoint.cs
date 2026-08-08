using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Api.Endpoints.Users.Dtos;
using Conduit.Identity.Application.Commands.UpdateUser;
using Conduit.Identity.Application.Queries.CurrentUser;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Application.Optional;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Conduit.Shared.Infrastructure.ErrorHandling;
using ErrorOr;
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

    private static Task<Results<Ok<UserEnvelope>, ProblemHttpResult>> HandleAsync(
        [Description("User details to update. At least one field is required.")]
        Request request,
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

        return mediator.Send(command, cancellationToken)
            .ThenAsync(_ => mediator.Send(new CurrentUserQuery(), cancellationToken))
            .Then(UserEnvelopeFactory.Create)
            .ToOkResult();
    }

    public sealed record Request
    {
        [Required]
        public required Data User { get; init; }

        public sealed record Data
        {
            public Optional<string> Username { get; init; }

            public Optional<string> Email { get; init; }

            public Optional<string> Password { get; init; }

            public Optional<string?> Bio { get; init; }

            public Optional<string?> Image { get; init; }
        }
    }
}
