namespace Conduit.UsersManagement.ApiEndpoints.Models;

internal sealed record LoginUser
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
