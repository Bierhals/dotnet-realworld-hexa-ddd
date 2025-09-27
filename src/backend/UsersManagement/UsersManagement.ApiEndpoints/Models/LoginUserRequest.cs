namespace Conduit.UsersManagement.ApiEndpoints.Models;

internal sealed record LoginUserRequest
{
    public required LoginUser User { get; init; }
}
