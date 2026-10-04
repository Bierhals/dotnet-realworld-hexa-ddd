using System.Collections.Generic;
using Conduit.Tags.Core.Domain;
using Conduit.Tags.Core.Domain.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conduit.Tags.Core.Infrastructure.Persistence.Configurations;

public sealed class ArticleTagUsageConfiguration : IEntityTypeConfiguration<ArticleTagUsage>
{
    public void Configure(EntityTypeBuilder<ArticleTagUsage> builder)
    {
        builder.ToTable("ArticleTagUsages");

        builder.HasKey(usage => usage.Id);
        builder.Property(usage => usage.Id).ValueGeneratedNever();

        // Two events about one article handled at the same time must not both build on the same
        // state: the second one fails, is retried, and then sees what the first one did.
        builder.Property(usage => usage.Revision).IsRequired().IsConcurrencyToken();
        builder.Property(usage => usage.IsDeleted).IsRequired();

        // The tag names are kept as one column: they are only ever read and replaced together.
        builder.Property<List<string>>("_tagNames").HasColumnName("TagNames").IsRequired();

        builder.Ignore(usage => usage.Tags);
        builder.Ignore(usage => usage.DomainEvents);
    }
}
