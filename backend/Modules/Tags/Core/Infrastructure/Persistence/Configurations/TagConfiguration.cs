using Conduit.Tags.Core.Domain;
using Conduit.Tags.Core.Domain.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conduit.Tags.Core.Infrastructure.Persistence.Configurations;

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(name => name.Value, value => TagName.Rehydrate(value))
            .HasMaxLength(TagNameLengthIsInRange.MaximumLength)
            .ValueGeneratedNever();

        // Several articles may reference one tag at the same time. A write that builds on a count
        // somebody else has changed since fails and is retried, instead of losing a use.
        builder.Property(t => t.ReferenceCount).IsRequired().IsConcurrencyToken();
    }
}
