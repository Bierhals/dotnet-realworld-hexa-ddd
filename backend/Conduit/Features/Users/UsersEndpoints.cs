using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Security;
using Conduit.Shared.RequestHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Features.Users;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var users = endpoints.MapGroup("/users")
            .WithTags("User and Authentication");

        users.MapPost("", CreateUserAsync)
            .WithSummary("Register a new user")
            .WithDescription("Register a new user<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#registration\">Conduit Spec for registration endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
        users.MapPost("login", LoginUserAsync)
            .WithSummary("Existing user login")
            .WithDescription("Login for existing user<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#authentication\">Conduit Spec for login endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);

        var currentUser = endpoints.MapGroup("/user")
            .WithTags("User and Authentication")
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtIssuerOptions.Schemes });

        currentUser.MapGet("", GetCurrentUserAsync)
            .WithSummary("Get current user")
            .WithDescription("Gets the currently logged-in user<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#get-current-user\">Conduit Spec for get current user endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
        currentUser.MapPut("", EditCurrentUserAsync)
            .WithSummary("Update current user")
            .WithDescription("Updated user information for current user<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#update-user\">Conduit Spec for update user endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }

    private static Task<UserEnvelope> CreateUserAsync(
        ICommandHandler<Create.Command, UserEnvelope> commandHandler,
        Create.Command command,
        CancellationToken cancellationToken
    ) => commandHandler.Handle(command, cancellationToken);

    private static Task<UserEnvelope> LoginUserAsync(
        ICommandHandler<Login.Command, UserEnvelope> commandHandler,
        Login.Command command,
        CancellationToken cancellationToken
    ) => commandHandler.Handle(command, cancellationToken);

    private static Task<UserEnvelope> GetCurrentUserAsync(
        IQueryHandler<Details.Query, UserEnvelope> queryHandler,
        ICurrentUserAccessor currentUserAccessor,
        CancellationToken cancellationToken
    ) => queryHandler.Handle(
        new Details.Query(currentUserAccessor.GetCurrentUsername() ?? "<unknown>"),
        cancellationToken
    );

    private static Task<UserEnvelope> EditCurrentUserAsync(
        ICommandHandler<Edit.Command, UserEnvelope> commandHandler,
        Edit.Command command,
        CancellationToken cancellationToken
    ) => commandHandler.Handle(command, cancellationToken);
}
