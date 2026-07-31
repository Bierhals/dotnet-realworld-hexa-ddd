using System.ComponentModel.DataAnnotations;
using Conduit.Shared.Application.Optional;

namespace Conduit.Identity.Api.Endpoints.Users;

public sealed record UpdateUserRequest
{
    [Required]
    public required UpdateUserData User { get; init; }
}

public sealed record UpdateUserData
{
    public Optional<string> Username { get; init; }

    public Optional<string> Email { get; init; }

    public Optional<string> Password { get; init; }

    public Optional<string?> Bio { get; init; }

    public Optional<string?> Image { get; init; }
}
