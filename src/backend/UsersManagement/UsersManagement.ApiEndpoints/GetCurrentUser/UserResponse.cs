namespace Conduit.UsersManagement.ApiEndpoints.GetCurrentUser;

/// <summary>
/// User
/// </summary>
public record UserResponse
{
    public required User User { get; init; }
}
