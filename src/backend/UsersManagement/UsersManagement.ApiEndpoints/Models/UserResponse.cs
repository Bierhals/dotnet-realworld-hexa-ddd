namespace Conduit.UsersManagement.ApiEndpoints.Models;

public record UserResponse
{
    public required User User { get; init; }
}
