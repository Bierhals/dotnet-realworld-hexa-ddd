using Conduit.Articles.Domain;
using Conduit.Articles.Domain.Rules;
using Conduit.Articles.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conduit.Articles.Infrastructure.Persistence.Configurations;

public sealed class ArticleFavoriteConfiguration : IEntityTypeConfiguration<ArticleFavorite>
{
    public void Configure(EntityTypeBuilder<ArticleFavorite> builder)
    {
        builder.ToTable("ArticleFavorites");

        builder.HasKey(favorite => favorite.Id);
        builder.Property(favorite => favorite.Id)
            .HasConversion(id => id.Value, value => ArticleFavoriteId.Rehydrate(value.ToString()))
            .ValueGeneratedNever();

        // Plain columns, not foreign keys: a favorite is its own aggregate and stays independent
        // of the article in the database too. Removing them together is the use case's job.
        builder.Property(favorite => favorite.ArticleId)
            .HasConversion(id => id.Value, value => ArticleId.Rehydrate(value.ToString()))
            .IsRequired();

        builder.Property(favorite => favorite.Username)
            .HasConversion(username => username.Value, value => AuthorUsername.Rehydrate(value))
            .HasMaxLength(AuthorUsernameLengthIsInRange.MaximumLength)
            .IsRequired();

        // An account can favorite an article once.
        builder.HasIndex(favorite => new { favorite.ArticleId, favorite.Username }).IsUnique();

        builder.Ignore(favorite => favorite.DomainEvents);
    }
}
