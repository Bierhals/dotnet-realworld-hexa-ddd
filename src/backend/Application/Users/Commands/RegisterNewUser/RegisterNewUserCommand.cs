using Conduit.Application.Users.Commands.Dtos;
using MediatR;

namespace Conduit.Application.Users.Commands.RegisterNewUser;

public class RegisterNewUserCommand : IRequest<UserDto>
{
    readonly NewUserDto _newUser;

    public RegisterNewUserCommand(NewUserDto newUser)
    {
        _newUser = newUser;
    }

    public string Email => _newUser.Email;
    public string Username => _newUser.Username;
    public string Password => _newUser.Password;
}
