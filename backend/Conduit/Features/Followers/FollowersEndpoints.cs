using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure.Security;
using MediatR;
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
            .WithTags("Followers")
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtIssuerOptions.Schemes });

        profiles.MapPost("{username}/follow", FollowUserAsync);
        profiles.MapDelete("{username}/follow", UnfollowUserAsync);

        return endpoints;
    }

    private static Task<Profiles.ProfileEnvelope> FollowUserAsync(
        IMediator mediator,
        [Required] string username,
        CancellationToken cancellationToken
    ) => mediator.Send(new Add.Command(username), cancellationToken);

    private static Task<Profiles.ProfileEnvelope> UnfollowUserAsync(
        IMediator mediator,
        [Required] string username,
        CancellationToken cancellationToken
    ) => mediator.Send(new Delete.Command(username), cancellationToken);
}
