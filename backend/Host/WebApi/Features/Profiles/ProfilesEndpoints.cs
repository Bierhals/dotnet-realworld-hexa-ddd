using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Host.WebApi.Shared.RequestHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Host.WebApi.Features.Profiles;

public static class ProfilesEndpoints
{
    public static IEndpointRouteBuilder MapProfilesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var profiles = endpoints.MapGroup("/profiles").WithTags("Profile");

        profiles.MapGet("{username}", GetProfileAsync)
            .WithSummary("Get a profile")
            .WithDescription("Get a profile of a user of the system. Auth is optional<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#get-profile\">Conduit Spec for get profile endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }

    private static Task<ProfileEnvelope> GetProfileAsync(
        [Required]
        [Description("Username of the profile to get")]
        string username,
        IQueryHandler<Details.Query, ProfileEnvelope> queryHandler,
        CancellationToken cancellationToken
    ) => queryHandler.Handle(new Details.Query(username), cancellationToken);
}
