using System.ComponentModel;

namespace Conduit.UsersManagement.ApiEndpoints.Models;

[Description("Details of the new user to register")]
internal sealed record NewUserRequest
{
    public required NewUser User { get; init; }
}
