namespace Conduit.UsersManagement.ApiEndpoints.Models;

public record NewUserRequest
{
    public required NewUser User { get; init; }
}
