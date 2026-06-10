using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Create = Conduit.Features.Users.Create;
using Details = Conduit.Features.Users.Details;
using Edit = Conduit.Features.Users.Edit;
using Login = Conduit.Features.Users.Login;

namespace Conduit.Features.Users;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var users = endpoints.MapGroup("/users")
            .WithTags("Users");

        users.MapPost("", CreateUserAsync);
        users.MapPost("login", LoginUserAsync);

        var currentUser = endpoints.MapGroup("/user")
            .WithTags("User")
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtIssuerOptions.Schemes });

        currentUser.MapGet("", GetCurrentUserAsync);
        currentUser.MapPut("", EditCurrentUserAsync);

        return endpoints;
    }

    private static Task<UserEnvelope> CreateUserAsync(
        IMediator mediator,
        Create.Command command,
        CancellationToken cancellationToken
    ) => mediator.Send(command, cancellationToken);

    private static Task<UserEnvelope> LoginUserAsync(
        IMediator mediator,
        Login.Command command,
        CancellationToken cancellationToken
    ) => mediator.Send(command, cancellationToken);

    private static Task<UserEnvelope> GetCurrentUserAsync(
        IMediator mediator,
        ICurrentUserAccessor currentUserAccessor,
        CancellationToken cancellationToken
    ) => mediator.Send(
        new Details.Query(currentUserAccessor.GetCurrentUsername() ?? "<unknown>"),
        cancellationToken
    );

    private static Task<UserEnvelope> EditCurrentUserAsync(
        IMediator mediator,
        Edit.Command command,
        CancellationToken cancellationToken
    ) => mediator.Send(command, cancellationToken);
}
