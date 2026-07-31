using System.Text.Json.Serialization;

namespace Conduit.Identity.Api.Endpoints.Users;

public sealed record UserResponse
{
    public required string Username { get; init; }

    public required string Email { get; init; }

    // The RealWorld spec requires bio/image to always be present in the response (as null when
    // unset), so the global JsonIgnoreCondition.WhenWritingNull must not drop these two.
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? Bio { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? Image { get; init; }

    public required string Token { get; init; }
}

public sealed record UserEnvelope(UserResponse User);
