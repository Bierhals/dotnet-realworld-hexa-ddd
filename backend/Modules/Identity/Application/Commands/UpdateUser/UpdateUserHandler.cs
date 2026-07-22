using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Application.Commands.UpdateUser;

public class UpdateUserHandler() : ICommandHandler<UpdateUserCommand>
{
    public Task<ErrorOr<Success>> Handle(UpdateUserCommand message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}