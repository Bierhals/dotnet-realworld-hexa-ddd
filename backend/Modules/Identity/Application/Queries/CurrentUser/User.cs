namespace Conduit.Application.Queries.CurrentUser;

public record User
{
    public required string Username { get; init; }

    public required string Email { get; init; }

    public string? Bio { get; init; }

    public string? Image { get; init; }

    public required string Token { get; set; }
}