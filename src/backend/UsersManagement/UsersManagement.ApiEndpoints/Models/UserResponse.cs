namespace Conduit.UsersManagement.ApiEndpoints.Models;

internal sealed record UserResponse
{
    public required User User { get; init; }
}
