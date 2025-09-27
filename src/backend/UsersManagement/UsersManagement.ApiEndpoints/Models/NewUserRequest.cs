namespace Conduit.UsersManagement.ApiEndpoints.Models;

internal sealed record NewUserRequest
{
    public required NewUser User { get; init; }
}
