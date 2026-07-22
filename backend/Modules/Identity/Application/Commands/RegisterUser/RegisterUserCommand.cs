using System;
using Conduit.Shared.Application.Cqrs;

namespace Conduit.Application.Commands.RegisterUser;

public record RegisterUserCommand() : ICommand<Guid>
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}
