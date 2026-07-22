using Conduit.Shared.Application.Cqrs;

namespace Conduit.Identity.Application.Commands.FollowUser;

public sealed record FollowUserCommand() : ICommand
{
    public required string Username { get; init; }
}
