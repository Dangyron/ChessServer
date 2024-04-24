using ChessServer.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessServer.Data.Common.ModelTypeConfigurations;

public class UserTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);
        builder.Property(u => u.Id)
            .ValueGeneratedNever();
        
        builder.HasIndex(user => user.Username).IsUnique();
        builder.HasIndex(user => user.Email).IsUnique();

        builder.Property(user => user.Age).IsRequired(false);
        builder.Property(user => user.Country).IsRequired(false);
        builder.Property(user => user.EloRating).HasDefaultValue(1500);
        
        builder.Property(user => user.Gender)
            .IsRequired(false).HasConversion(gender => gender!.Value, gender => Gender.FromValue(gender));

        builder.OwnsOne(u => u.Subscription, sb =>
        {
            sb.Property(s => s.Id)
                .HasColumnName("SubscriptionId");

            sb.Property(s => s.SubscriptionType)
                .HasConversion(
                    v => v.Name,
                    v => SubscriptionType.FromName(v, false));
        });

        builder.Property(user => user.EmailConfirmed)
            .HasDefaultValue(false);

        builder.Property(user => user.Password).IsRequired();
    }
}