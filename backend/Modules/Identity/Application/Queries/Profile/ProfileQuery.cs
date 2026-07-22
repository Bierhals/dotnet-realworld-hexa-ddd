using Conduit.Shared.Application.Cqrs;

namespace Conduit.Identity.Application.Queries.Profile;

public sealed record ProfileQuery() : IQuery<Profile>
{
    public required string Username { get; init; }
}

