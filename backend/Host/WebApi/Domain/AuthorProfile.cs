namespace Conduit.Host.WebApi.Domain;

public class AuthorProfile
{
    public required string Username { get; init; }

    public string? Bio { get; init; }

    public string? Image { get; init; }

    public bool Following { get; init; }
}
