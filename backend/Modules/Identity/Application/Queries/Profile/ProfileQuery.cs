using Conduit.Shared.Application.Cqrs;

namespace Conduit.Application.Queries.Profile;

public record ProfileQuery() : IQuery<Profile>
{
    public required string Username { get; init; }
}

