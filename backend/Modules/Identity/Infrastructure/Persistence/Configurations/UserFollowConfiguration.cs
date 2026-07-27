using Conduit.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conduit.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserFollowConfiguration : IEntityTypeConfiguration<UserFollow>
{
    public void Configure(EntityTypeBuilder<UserFollow> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasConversion(id => id.Value, value => UserFollowId.Rehydrate(value.ToString()))
            .ValueGeneratedNever();

        builder.Property(f => f.FollowedUserId)
            .HasConversion(id => id.Value, value => UserId.Rehydrate(value.ToString()));
        builder.Property(f => f.FollowerUserId)
            .HasConversion(id => id.Value, value => UserId.Rehydrate(value.ToString()));

        builder.HasIndex(f => new { f.FollowedUserId, f.FollowerUserId }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.FollowedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.FollowerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
