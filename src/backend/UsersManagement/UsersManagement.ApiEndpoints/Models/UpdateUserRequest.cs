namespace Conduit.UsersManagement.ApiEndpoints.Models;

public sealed record UpdateUserRequest
{
    public required UpdateUser User { get; init; }
}
