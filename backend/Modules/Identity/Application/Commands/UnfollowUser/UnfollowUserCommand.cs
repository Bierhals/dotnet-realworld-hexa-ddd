using System;
using Conduit.Shared.Application.Cqrs;

namespace Conduit.Application.Commands.UnfollowUser;

public record UnfollowUserCommand() : ICommand
{
    public required string Username { get; init; }
}