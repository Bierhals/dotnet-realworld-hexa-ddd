using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conduit.Shared.Infrastructure.Outbox;

/// <summary>
/// Shared column mapping applied by each module's OnModelCreating. Not discovered via
/// ApplyConfigurationsFromAssembly - that only scans each module's own assembly by design (module
/// isolation), so this is registered explicitly per module instead.
/// </summary>
public static class OutboxDomainEventConfiguration
{
    private const string TableName = "OutboxDomainEvents";

    /// <summary>
    /// Each module's own schema already disambiguates "OutboxDomainEvents" under a provider with
    /// real schemas (Postgres). SQLite has no such concept - it ignores the schema each DbContext
    /// declares - and modules can share one physical SQLite file, so there the table name is
    /// prefixed per module instead to avoid a collision.
    /// </summary>
    public static void Configure(EntityTypeBuilder<OutboxDomainEvent> builder, string? providerName, string moduleName)
    {
        var tableName = SupportsSchemas(providerName) ? TableName : $"{moduleName}{TableName}";
        builder.ToTable(tableName);
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.Property(m => m.DomainEventId).IsRequired();
        builder.Property(m => m.DomainEventType).IsRequired();
        builder.Property(m => m.HandlerType).IsRequired();
        builder.Property(m => m.Payload).IsRequired();
        builder.Property(m => m.OccurredOnUtc).IsRequired();

        // W3C traceparent: "version-traceid-spanid-flags", 55 chars fixed; headroom for safety.
        builder.Property(m => m.TraceParent).HasMaxLength(64);

        // Filtered by every poll cycle's claim UPDATE (unprocessed, unclaimed-or-lease-expired).
        builder.HasIndex(m => new { m.ProcessedOnUtc, m.ClaimedAtUtc });

        // Lets an operator find every handler-row that fanned out from the same domain event.
        builder.HasIndex(m => m.DomainEventId);
    }

    private static bool SupportsSchemas(string? providerName) =>
        providerName is not null && providerName.Contains("PostgreSQL", StringComparison.OrdinalIgnoreCase);
}
