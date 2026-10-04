namespace Conduit.Shared.Infrastructure.IntegrationTests;

/// <summary>
/// Every test class that starts a Wolverine host belongs here. Hosts in one process interfere with
/// each other's tracked sessions and static test state, so these classes must not run in parallel.
/// </summary>
[CollectionDefinition(Name)]
public sealed class WolverineHostCollection
{
    public const string Name = "Wolverine host";
}
