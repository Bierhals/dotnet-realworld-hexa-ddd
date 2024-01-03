namespace Conduit.RestAPI;

public record class UpdateUserRequest
{
    public required UpdateUser User { get; init; }
}
