using System;
using Conduit.Shared.Application.Cqrs;

namespace Conduit.Application.Commands.AuthenticateUser;

public record AuthenticateUserCommand() : ICommand<Guid>
{
    public required string Email { get; init; }

    public required string Password { get; init; }
}
