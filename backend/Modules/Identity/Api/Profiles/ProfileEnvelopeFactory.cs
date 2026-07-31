using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application.Queries.Profile;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Conduit.Identity.Api.Endpoints.Profiles;

internal static class ProfileEnvelopeFactory
{
    public static async Task<Results<Ok<ProfileEnvelope>, ProblemHttpResult>> BuildAsync(
        string username,
        ICqrsMediator mediator,
        CancellationToken cancellationToken)
    {
        var profile = await mediator.Send(new ProfileQuery { Username = username }, cancellationToken);
        if (profile.IsError)
        {
            return profile.Errors.ToProblemResult();
        }

        return TypedResults.Ok(new ProfileEnvelope(new ProfileResponse
        {
            Username = profile.Value.Username,
            Bio = profile.Value.Bio,
            Image = profile.Value.Image,
            Following = profile.Value.Following,
        }));
    }
}
