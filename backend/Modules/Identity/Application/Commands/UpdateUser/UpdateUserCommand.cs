using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Application.Optional;

namespace Conduit.Identity.Application.Commands.UpdateUser;

public sealed record UpdateUserCommand() : ICommand
{
    public Optional<string> Username { get; set; }

    public Optional<string> Email { get; set; }

    public Optional<string> Password { get; set; }

    public Optional<string?> Bio { get; set; }

    public Optional<string?> Image { get; set; }
}
