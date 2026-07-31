using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Identity.Api.Endpoints.Profiles;

internal sealed class GetProfileEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{username}", HandleAsync)
            .WithSummary("Get a profile")
            .WithDescription("Get a profile of a user of the system. Auth is optional<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#get-profile\">Conduit Spec for get profile endpoint</a>")
            .Produces<ProfileEnvelope>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static Task<Results<Ok<ProfileEnvelope>, ProblemHttpResult>> HandleAsync(
        [Required]
        [Description("Username of the profile to get")]
        string username,
        ICqrsMediator mediator,
        CancellationToken cancellationToken
    ) => ProfileEnvelopeFactory.BuildAsync(username, mediator, cancellationToken);
}
