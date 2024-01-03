namespace Conduit.RestAPI.ViewModels;

public record ProfileResponse
{
    public required Profile Profile { get; init; }
}
