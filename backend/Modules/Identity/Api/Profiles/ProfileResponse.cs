using System.Text.Json.Serialization;

namespace Conduit.Identity.Api.Endpoints.Profiles;

public sealed record ProfileResponse
{
    public required string Username { get; init; }

    // The RealWorld spec requires bio/image to always be present in the response (as null when
    // unset), so the global JsonIgnoreCondition.WhenWritingNull must not drop these two.
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? Bio { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? Image { get; init; }

    public required bool Following { get; init; }
}

public sealed record ProfileEnvelope(ProfileResponse Profile);
