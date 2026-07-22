using System;
using Conduit.Shared.Application.Cqrs;

namespace Conduit.Application.Commands.FollowUser;

public record FollowUserCommand() : ICommand
{
    public required string Username { get; init; }
}
