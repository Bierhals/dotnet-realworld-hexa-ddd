using System;

namespace Conduit.Shared.Infrastructure.Outbox;

public sealed class OutboxProcessorOptions
{
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(2);

    public int BatchSize { get; set; } = 20;

    public int MaxRetries { get; set; } = 5;

    /// <summary>
    /// How long a claim on a row is honored before another instance is allowed to reclaim it.
    /// Must comfortably exceed how long a batch normally takes to dispatch - it only kicks in when
    /// an instance claimed a row and then crashed or hung before releasing it.
    /// </summary>
    public TimeSpan ClaimLeaseDuration { get; set; } = TimeSpan.FromSeconds(30);
}
