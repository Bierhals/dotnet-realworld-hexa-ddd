using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application.Queries.Profile;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Identity.Api.Endpoints.Profiles.Dtos;

internal static class ProfileEnvelopeFactory
{
    public static Task<ErrorOr<ProfileEnvelope>> BuildAsync(
        string username,
        ICqrsMediator mediator,
        CancellationToken cancellationToken) =>
        mediator.Send(new ProfileQuery { Username = username }, cancellationToken)
            .Then(profile => new ProfileEnvelope(new ProfileResponse
            {
                Username = profile.Username,
                Bio = profile.Bio,
                Image = profile.Image,
                Following = profile.Following,
            }));
}
