namespace Conduit.UsersManagement.ApiEndpoints.Models;

public sealed record NewUserRequest
{
    public required NewUser User { get; init; }
}
