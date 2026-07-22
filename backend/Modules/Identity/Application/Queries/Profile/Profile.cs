namespace Conduit.Identity.Application.Queries.Profile;

public sealed record Profile
{
    public required string Username { get; init; }

    public string? Bio { get; init; }

    public string? Image { get; init; }

    public required bool Following { get; set; }
}