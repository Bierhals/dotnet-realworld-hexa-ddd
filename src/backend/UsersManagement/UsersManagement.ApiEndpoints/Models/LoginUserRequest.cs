namespace Conduit.UsersManagement.ApiEndpoints.Models;

public sealed record LoginUserRequest
{
    public required LoginUser User { get; init; }
}
