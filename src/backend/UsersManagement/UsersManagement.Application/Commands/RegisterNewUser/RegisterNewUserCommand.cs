using Conduit.Users.Application.Users.Dtos;
using Conduit.Domain.Common;
using CSharpFunctionalExtensions;
using MediatR;

namespace Conduit.Users.Application.Users.Commands.RegisterNewUser;

public class RegisterNewUserCommand : IRequest<Result<UserDto, Error>>
{
    public required string Email { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}
