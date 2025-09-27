namespace Conduit.UsersManagement.ApiEndpoints.Models;

internal sealed record UpdateUser
{
    public string? Email { get; init; }
    public string? Password { get; init; }
    public string? Username { get; init; }
    public string? Bio { get; init; }
    public string? Image { get; init; }
}
