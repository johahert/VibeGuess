using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeGuess.Core.Entities;

namespace VibeGuess.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for QuizSession entity.
/// </summary>
public class QuizSessionConfiguration : IEntityTypeConfiguration<QuizSession>
{
    public void Configure(EntityTypeBuilder<QuizSession> builder)
    {
        // Table name
        builder.ToTable("QuizSessions");

        // Primary key
        builder.HasKey(qs => qs.Id);

        // Properties
        builder.Property(qs => qs.DeviceId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(qs => qs.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Active");

        builder.Property(qs => qs.CurrentScore)
            .HasDefaultValue(0);

        builder.Property(qs => qs.ShuffleQuestions)
            .HasDefaultValue(false);

        builder.Property(qs => qs.EnableHints)
            .HasDefaultValue(true);

        builder.Property(qs => qs.SessionConfig)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(qs => qs.QuizId)
            .HasDatabaseName("IX_QuizSessions_QuizId");

        builder.HasIndex(qs => qs.UserId)
            .HasDatabaseName("IX_QuizSessions_UserId");

        builder.HasIndex(qs => new { qs.UserId, qs.Status })
            .HasDatabaseName("IX_QuizSessions_UserId_Status");

        builder.HasIndex(qs => new { qs.Status, qs.ExpiresAt })
            .HasDatabaseName("IX_QuizSessions_Status_ExpiresAt");

        builder.HasIndex(qs => qs.StartedAt)
            .HasDatabaseName("IX_QuizSessions_StartedAt");

        // Relationships
        builder.HasMany(qs => qs.UserAnswers)
            .WithOne(ua => ua.QuizSession)
            .HasForeignKey(ua => ua.QuizSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Quiz and User relationships are configured in their respective configurations
    }
}