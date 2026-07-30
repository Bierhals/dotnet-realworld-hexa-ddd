namespace Conduit.Identity.Api.Endpoints.Profiles;

public sealed record ProfileResponse
{
    public required string Username { get; init; }

    public string? Bio { get; init; }

    public string? Image { get; init; }

    public required bool Following { get; init; }
}

public sealed record ProfileEnvelope(ProfileResponse Profile);
