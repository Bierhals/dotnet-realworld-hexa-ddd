using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Application.Commands.RegisterUser;

public class RegisterUserHandler() : ICommandHandler<RegisterUserCommand, Guid>
{
    public Task<ErrorOr<Guid>> Handle(RegisterUserCommand message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}