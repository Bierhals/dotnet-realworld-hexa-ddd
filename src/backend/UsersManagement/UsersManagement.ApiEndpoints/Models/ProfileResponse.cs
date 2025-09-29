using System.ComponentModel;

namespace Conduit.UsersManagement.ApiEndpoints.Models;

[Description("Profile")]
public record ProfileResponse
{
    public required Profile Profile { get; init; }
}
