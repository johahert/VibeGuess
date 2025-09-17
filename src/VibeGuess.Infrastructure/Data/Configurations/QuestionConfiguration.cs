using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeGuess.Core.Entities;

namespace VibeGuess.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Question entity.
/// </summary>
public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        // Table name
        builder.ToTable("Questions");

        // Primary key
        builder.HasKey(qu => qu.Id);

        // Properties
        builder.Property(qu => qu.QuestionText)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(qu => qu.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(qu => qu.RequiresAudio)
            .HasDefaultValue(true);

        builder.Property(qu => qu.Points)
            .HasDefaultValue(10);

        builder.Property(qu => qu.HintText)
            .HasMaxLength(200);

        builder.Property(qu => qu.Explanation)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(qu => qu.QuizId)
            .HasDatabaseName("IX_Questions_QuizId");

        builder.HasIndex(qu => new { qu.QuizId, qu.OrderIndex })
            .IsUnique()
            .HasDatabaseName("IX_Questions_QuizId_OrderIndex");

        // Relationships
        builder.HasMany(qu => qu.AnswerOptions)
            .WithOne(ao => ao.Question)
            .HasForeignKey(ao => ao.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(qu => qu.UserAnswers)
            .WithOne(ua => ua.Question)
            .HasForeignKey(ua => ua.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(qu => qu.Track)
            .WithMany(t => t.Questions)
            .HasForeignKey("TrackId") // Shadow property
            .OnDelete(DeleteBehavior.SetNull);

        // Quiz relationship is configured in QuizConfiguration
    }
}