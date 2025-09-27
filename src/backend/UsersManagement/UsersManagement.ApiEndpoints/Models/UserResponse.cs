namespace Conduit.UsersManagement.ApiEndpoints.Models;

public sealed record UserResponse
{
    public required User User { get; init; }
}
