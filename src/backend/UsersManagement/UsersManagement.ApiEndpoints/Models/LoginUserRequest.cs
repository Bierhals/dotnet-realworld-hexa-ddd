using System.ComponentModel;

namespace Conduit.UsersManagement.ApiEndpoints.Models;

[Description("Credentials to use")]
internal sealed record LoginUserRequest
{
    public required LoginUser User { get; init; }
}
