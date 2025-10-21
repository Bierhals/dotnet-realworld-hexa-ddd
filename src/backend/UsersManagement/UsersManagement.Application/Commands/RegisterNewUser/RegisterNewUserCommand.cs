
using Conduit.Shared.RequestHandling;
using Conduit.UsersManagement.Application.Dtos;
using ErrorOr;

namespace Conduit.UsersManagement.Application.Commands.RegisterNewUser;

public class RegisterNewUserCommand : ICommand<ErrorOr<UserDto>>
{
    public required string Email { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}
