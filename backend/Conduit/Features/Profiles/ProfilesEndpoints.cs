using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Features.Profiles;

public static class ProfilesEndpoints
{
    public static IEndpointRouteBuilder MapProfilesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var profiles = endpoints.MapGroup("/profiles").WithTags("Profiles");

        profiles.MapGet("{username}", GetProfileAsync);

        return endpoints;
    }

    private static Task<ProfileEnvelope> GetProfileAsync(
        IMediator mediator,
        [Required] string username,
        CancellationToken cancellationToken
    ) => mediator.Send(new Details.Query(username), cancellationToken);
}
