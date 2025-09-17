using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeGuess.Core.Entities;

namespace VibeGuess.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for User entity.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table name
        builder.ToTable("Users");

        // Primary key
        builder.HasKey(u => u.Id);

        // Properties
        builder.Property(u => u.SpotifyUserId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(u => u.Country)
            .HasMaxLength(2);

        builder.Property(u => u.ProfileImageUrl)
            .HasMaxLength(500);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("User");

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(u => u.SpotifyUserId)
            .IsUnique()
            .HasDatabaseName("IX_Users_SpotifyUserId");

        builder.HasIndex(u => u.Email)
            .HasDatabaseName("IX_Users_Email");

        builder.HasIndex(u => new { u.Role, u.IsActive })
            .HasDatabaseName("IX_Users_Role_IsActive");

        // Relationships
        builder.HasOne(u => u.Settings)
            .WithOne(s => s.User)
            .HasForeignKey<UserSettings>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.CreatedQuizzes)
            .WithOne(q => q.User)
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.QuizSessions)
            .WithOne(qs => qs.User)
            .HasForeignKey(qs => qs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.SpotifyTokens)
            .WithOne(st => st.User)
            .HasForeignKey(st => st.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}