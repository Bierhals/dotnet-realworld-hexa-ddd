using System.Diagnostics;

namespace Conduit.Shared.Infrastructure.Outbox;

/// <summary>
/// Tracing source for outbox dispatch spans. The name must match the source registered via
/// tracing.AddSource(...) in Host/ServiceDefaults/Extensions.cs, or dispatch spans are created but
/// never collected/exported (StartActivity returns null without a listener either way, so a
/// mismatch fails silently rather than throwing).
/// </summary>
public static class OutboxActivitySource
{
    public const string Name = "Conduit.Outbox";

    public static readonly ActivitySource Instance = new(Name);
}
