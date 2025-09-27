using System.ComponentModel;

namespace Conduit.UsersManagement.ApiEndpoints.Models;

[Description("User details to update. At least **one** field is required.")]
internal sealed record UpdateUserRequest
{
    public required UpdateUser User { get; init; }
}
