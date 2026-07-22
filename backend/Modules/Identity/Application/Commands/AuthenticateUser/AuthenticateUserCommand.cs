using System;
using Conduit.Shared.Application.Cqrs;

namespace Conduit.Identity.Application.Commands.AuthenticateUser;

public sealed record AuthenticateUserCommand() : ICommand<Guid>
{
    public required string Email { get; init; }

    public required string Password { get; init; }
}
