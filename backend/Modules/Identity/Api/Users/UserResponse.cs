namespace Conduit.Identity.Api.Endpoints.Users;

public sealed record UserResponse
{
    public required string Username { get; init; }

    public required string Email { get; init; }

    public string? Bio { get; init; }

    public string? Image { get; init; }

    public required string Token { get; init; }
}

public sealed record UserEnvelope(UserResponse User);
