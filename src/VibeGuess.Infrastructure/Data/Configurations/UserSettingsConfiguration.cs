using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeGuess.Core.Entities;

namespace VibeGuess.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for UserSettings entity.
/// </summary>
public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        // Table name
        builder.ToTable("UserSettings");

        // Primary key
        builder.HasKey(us => us.Id);

        // Properties
        builder.Property(us => us.PreferredLanguage)
            .IsRequired()
            .HasMaxLength(5)
            .HasDefaultValue("en");

        builder.Property(us => us.DefaultQuestionCount)
            .HasDefaultValue(10);

        builder.Property(us => us.DefaultDifficulty)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Medium");

        builder.Property(us => us.LastSelectedDeviceId)
            .HasMaxLength(100);

        builder.Property(us => us.EnableAudioPreview)
            .HasDefaultValue(true);

        builder.Property(us => us.RememberDeviceSelection)
            .HasDefaultValue(true);

        builder.Property(us => us.EnableHints)
            .HasDefaultValue(true);

        builder.Property(us => us.ShuffleQuestions)
            .HasDefaultValue(false);

        // Foreign key
        builder.HasIndex(us => us.UserId)
            .IsUnique()
            .HasDatabaseName("IX_UserSettings_UserId");

        // Relationships are configured in UserConfiguration
    }
}