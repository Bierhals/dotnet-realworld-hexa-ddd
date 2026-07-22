using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Identity.Application.Commands.UnfollowUser;

public sealed class UnfollowUserHandler() : ICommandHandler<UnfollowUserCommand>
{
    public Task<ErrorOr<Success>> Handle(UnfollowUserCommand message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}