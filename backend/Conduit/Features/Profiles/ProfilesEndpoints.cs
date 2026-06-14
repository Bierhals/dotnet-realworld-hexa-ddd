using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.RequestHandling;
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
        IQueryHandler<Details.Query, ProfileEnvelope> queryHandler,
        [Required] string username,
        CancellationToken cancellationToken
    ) => queryHandler.Handle(new Details.Query(username), cancellationToken);
}
