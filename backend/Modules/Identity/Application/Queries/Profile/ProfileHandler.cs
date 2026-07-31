using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Identity.Application.Queries.Profile;

public sealed class ProfileHandler(ICurrentUserAccessor currentUserAccessor, IUsersReadRepository usersReadRepository) : IQueryHandler<ProfileQuery, Profile>
{
    public Task<ErrorOr<Profile>> Handle(ProfileQuery message, CancellationToken cancellationToken) =>
        usersReadRepository.GetProfileAsync(message.Username, currentUserAccessor.GetCurrentUsername(), cancellationToken);
}
