using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Application.Users.Commands.Dtos;
using MediatR;

namespace Conduit.Application.Users.Commands.RegisterNewUser;

public class RegisterNewUserHandler : IRequestHandler<RegisterNewUserCommand, UserDto>
{
    public Task<UserDto> Handle(RegisterNewUserCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
