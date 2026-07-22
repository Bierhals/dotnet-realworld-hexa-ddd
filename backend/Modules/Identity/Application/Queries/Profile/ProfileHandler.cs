using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Application.Queries.Profile;

public class ProfileHandler() : IQueryHandler<ProfileQuery, Profile>
{
    public Task<ErrorOr<Profile>> Handle(ProfileQuery message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}