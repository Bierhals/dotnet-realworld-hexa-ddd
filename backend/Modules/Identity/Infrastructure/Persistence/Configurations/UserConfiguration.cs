using Conduit.Identity.Domain;
using Conduit.Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conduit.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, value => UserId.Rehydrate(value.ToString()))
            .ValueGeneratedNever();

        builder.Property(u => u.Username)
            .HasConversion(username => username.Value, value => Username.Rehydrate(value))
            .IsRequired();
        builder.HasIndex(u => u.Username).IsUnique();

        builder.Property(u => u.Email)
            .HasConversion(email => email.Value, value => UserEmail.Rehydrate(value))
            .IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Image)
            .HasConversion(
                image => image == null ? null : image.Value,
                value => value == null ? null : UserImage.Rehydrate(value));

        builder.Property(u => u.PasswordHash).IsRequired();
    }
}
