using System.ComponentModel.DataAnnotations;

namespace Conduit.Identity.Api.Endpoints.Users;

public sealed record LoginUserRequest
{
    [Required]
    public required LoginUserData User { get; init; }
}

public sealed record LoginUserData
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}
