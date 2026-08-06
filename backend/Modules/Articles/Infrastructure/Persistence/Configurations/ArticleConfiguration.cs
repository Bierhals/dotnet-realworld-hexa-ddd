using Conduit.Articles.Domain;
using Conduit.Articles.Domain.Rules;
using Conduit.Articles.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conduit.Articles.Infrastructure.Persistence.Configurations;

public sealed class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("Articles");

        builder.HasKey(article => article.Id);
        builder.Property(article => article.Id)
            .HasConversion(id => id.Value, value => ArticleId.Rehydrate(value.ToString()))
            .ValueGeneratedNever();

        builder.Property(article => article.Slug)
            .HasConversion(slug => slug.Value, value => ArticleSlug.Rehydrate(value))
            .HasMaxLength(ArticleSlug.MaximumLength)
            .IsRequired();
        builder.HasIndex(article => article.Slug);

        builder.Property(article => article.Title)
            .HasConversion(title => title.Value, value => ArticleTitle.Rehydrate(value))
            .HasMaxLength(ArticleTitleIsValid.MaximumLength)
            .IsRequired();

        builder.Property(article => article.Description)
            .HasConversion(description => description.Value, value => ArticleDescription.Rehydrate(value))
            .HasMaxLength(ArticleDescriptionIsValid.MaximumLength)
            .IsRequired();

        builder.Property(article => article.Body)
            .HasConversion(body => body.Value, value => ArticleBody.Rehydrate(value))
            .IsRequired();

        builder.Property(article => article.Author)
            .HasConversion(author => author.Value, value => AuthorUsername.Rehydrate(value))
            .HasMaxLength(AuthorUsernameIsValid.MaximumLength)
            .IsRequired();
        builder.HasIndex(article => article.Author);

        builder.Property(article => article.CreatedAt).IsRequired();
        builder.Property(article => article.UpdatedAt).IsRequired();

        // A tag on an article is just a name, so the aggregate holds plain tag names and the
        // mapping - not the domain - is what spreads them over their own table.
        builder.OwnsMany(article => article.Tags, tag =>
        {
            tag.ToTable("ArticleTags");
            tag.WithOwner().HasForeignKey("ArticleId");
            tag.Property(tagName => tagName.Value)
                .HasColumnName("TagName")
                .HasMaxLength(TagNameIsValid.MaximumLength);
            tag.HasKey("ArticleId", "Value");
        });
        builder.Navigation(article => article.Tags).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(article => article.DomainEvents);
    }
}
