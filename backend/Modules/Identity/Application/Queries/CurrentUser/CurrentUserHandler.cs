using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Identity.Application.Queries.CurrentUser;

public sealed class CurrentUserHandler() : IQueryHandler<CurrentUserQuery, User>
{
    public Task<ErrorOr<User>> Handle(CurrentUserQuery message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}