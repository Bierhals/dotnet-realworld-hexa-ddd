using System.ComponentModel.DataAnnotations;

namespace Conduit.Identity.Api.Endpoints.Users;

public sealed record RegisterUserRequest
{
    [Required]
    public required RegisterUserData User { get; init; }
}

public sealed record RegisterUserData
{
    [Required]
    public required string Username { get; init; }

    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}
