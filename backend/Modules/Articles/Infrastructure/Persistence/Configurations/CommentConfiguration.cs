using Conduit.Articles.Domain;
using Conduit.Articles.Domain.Rules;
using Conduit.Articles.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conduit.Articles.Infrastructure.Persistence.Configurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        // The number comes from the comment-number sequence, not from an identity column, so that
        // the use case can hand it to the aggregate before anything is written.
        builder.HasKey(comment => comment.Id);
        builder.Property(comment => comment.Id)
            .HasConversion(id => id.Value, value => CommentId.From(value))
            .ValueGeneratedNever();

        // A plain column, not a foreign key: a comment is its own aggregate and stays independent
        // of the article in the database too. Removing them together is the use case's job.
        builder.Property(comment => comment.ArticleId)
            .HasConversion(id => id.Value, value => ArticleId.Rehydrate(value.ToString()))
            .IsRequired();
        builder.HasIndex(comment => comment.ArticleId);

        builder.Property(comment => comment.Body)
            .HasConversion(body => body.Value, value => CommentBody.Rehydrate(value))
            .IsRequired();

        builder.Property(comment => comment.Author)
            .HasConversion(author => author.Value, value => Username.Rehydrate(value))
            .HasMaxLength(UsernameLengthIsInRange.MaximumLength)
            .IsRequired();

        builder.Property(comment => comment.CreatedAtUtc).IsRequired();
        builder.Property(comment => comment.UpdatedAtUtc).IsRequired();

        builder.Ignore(comment => comment.DomainEvents);
    }
}
