namespace Conduit.Identity.Application.Queries.CurrentUser;

public sealed record User
{
    public required string Username { get; init; }

    public required string Email { get; init; }

    public string? Bio { get; init; }

    public string? Image { get; init; }

    public required string Token { get; set; }
}