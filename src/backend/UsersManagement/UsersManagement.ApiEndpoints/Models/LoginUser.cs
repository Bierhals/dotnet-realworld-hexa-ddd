namespace Conduit.UsersManagement.ApiEndpoints.Models;

public sealed record LoginUser
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
