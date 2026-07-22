using Conduit.Shared.Application.Cqrs;

namespace Conduit.Identity.Application.Commands.UpdateUser;

public sealed record UpdateUserCommand() : ICommand
{
    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Bio { get; set; }

    public string? Image { get; set; }
}
