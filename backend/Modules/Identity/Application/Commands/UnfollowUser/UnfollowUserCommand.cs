using Conduit.Shared.Application.Cqrs;

namespace Conduit.Identity.Application.Commands.UnfollowUser;

public sealed record UnfollowUserCommand() : ICommand
{
    public required string Username { get; init; }
}