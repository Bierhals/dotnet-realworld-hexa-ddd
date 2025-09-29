namespace Conduit.ContentManagement.ApiEndpoints.Models;

public record Author
{
    public required string Username { get; init; }
    public required string Bio { get; init; }
    public required string Image { get; init; }
    public required bool Following { get; init; }
}
