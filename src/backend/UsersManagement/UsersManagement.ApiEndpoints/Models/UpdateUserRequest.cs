namespace Conduit.UsersManagement.ApiEndpoints.Models;

internal sealed record UpdateUserRequest
{
    public required UpdateUser User { get; init; }
}
