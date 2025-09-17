using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeGuess.Core.Entities;

namespace VibeGuess.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for UserAnswer entity.
/// </summary>
public class UserAnswerConfiguration : IEntityTypeConfiguration<UserAnswer>
{
    public void Configure(EntityTypeBuilder<UserAnswer> builder)
    {
        // Table name
        builder.ToTable("UserAnswers");

        // Primary key
        builder.HasKey(ua => ua.Id);

        // Properties
        builder.Property(ua => ua.FreeTextAnswer)
            .HasMaxLength(500);

        builder.Property(ua => ua.IsCorrect)
            .HasDefaultValue(false);

        builder.Property(ua => ua.PointsEarned)
            .HasDefaultValue(0);

        builder.Property(ua => ua.UsedHint)
            .HasDefaultValue(false);

        builder.Property(ua => ua.PlaybackCount)
            .HasDefaultValue(0);

        builder.Property(ua => ua.AnswerMetadata)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(ua => ua.QuizSessionId)
            .HasDatabaseName("IX_UserAnswers_QuizSessionId");

        builder.HasIndex(ua => ua.QuestionId)
            .HasDatabaseName("IX_UserAnswers_QuestionId");

        builder.HasIndex(ua => new { ua.QuizSessionId, ua.QuestionId })
            .IsUnique()
            .HasDatabaseName("IX_UserAnswers_QuizSessionId_QuestionId");

        builder.HasIndex(ua => ua.AnsweredAt)
            .HasDatabaseName("IX_UserAnswers_AnsweredAt");

        builder.HasIndex(ua => new { ua.IsCorrect, ua.PointsEarned })
            .HasDatabaseName("IX_UserAnswers_IsCorrect_PointsEarned");

        // Relationships are configured in other configurations
    }
}