using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Application.Commands.FollowUser;

public class FollowUserHandler() : ICommandHandler<FollowUserCommand>
{
    public Task<ErrorOr<Success>> Handle(FollowUserCommand message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}