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

        builder.Property(favorite => favorite.ArticleId)
            .HasConversion(id => id.Value, value => ArticleId.Rehydrate(value.ToString()));

        builder.Property(favorite => favorite.Username)
            .HasConversion(username => username.Value, value => AuthorUsername.Rehydrate(value))
            .HasMaxLength(AuthorUsernameIsValid.MaximumLength);

        builder.HasKey(favorite => new { favorite.ArticleId, favorite.Username });
    }
}
