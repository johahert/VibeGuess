using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeGuess.Core.Entities;

namespace VibeGuess.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for AnswerOption entity.
/// </summary>
public class AnswerOptionConfiguration : IEntityTypeConfiguration<AnswerOption>
{
    public void Configure(EntityTypeBuilder<AnswerOption> builder)
    {
        // Table name
        builder.ToTable("AnswerOptions");

        // Primary key
        builder.HasKey(ao => ao.Id);

        // Properties
        builder.Property(ao => ao.OptionLabel)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(ao => ao.AnswerText)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ao => ao.IsCorrect)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(ao => ao.QuestionId)
            .HasDatabaseName("IX_AnswerOptions_QuestionId");

        builder.HasIndex(ao => new { ao.QuestionId, ao.OrderIndex })
            .IsUnique()
            .HasDatabaseName("IX_AnswerOptions_QuestionId_OrderIndex");

        builder.HasIndex(ao => new { ao.QuestionId, ao.IsCorrect })
            .HasDatabaseName("IX_AnswerOptions_QuestionId_IsCorrect");

        // Relationships
        builder.HasMany(ao => ao.UserAnswers)
            .WithOne(ua => ua.SelectedAnswerOption)
            .HasForeignKey(ua => ua.SelectedAnswerOptionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Question relationship is configured in QuestionConfiguration
    }
}