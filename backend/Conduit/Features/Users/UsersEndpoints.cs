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

        users.MapPost("", CreateUserAsync);
        users.MapPost("login", LoginUserAsync)
            .WithSummary("Existing user login")
            .WithDescription("Login for existing user<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#authentication\">Conduit Spec for login endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);

        var currentUser = endpoints.MapGroup("/user")
            .WithTags("User and Authentication")
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtIssuerOptions.Schemes });

        currentUser.MapGet("", GetCurrentUserAsync);
        currentUser.MapPut("", EditCurrentUserAsync);

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
