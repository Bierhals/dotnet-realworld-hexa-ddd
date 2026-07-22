using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Identity.Application.Commands.AuthenticateUser;

public sealed class AuthenticateUserHandler() : ICommandHandler<AuthenticateUserCommand, Guid>
{
    public Task<ErrorOr<Guid>> Handle(AuthenticateUserCommand message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}