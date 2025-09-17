using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeGuess.Core.Entities;

namespace VibeGuess.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Quiz entity.
/// </summary>
public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        // Table name
        builder.ToTable("Quizzes");

        // Primary key
        builder.HasKey(q => q.Id);

        // Properties
        builder.Property(q => q.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(q => q.UserPrompt)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(q => q.Format)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(q => q.Difficulty)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(q => q.Language)
            .IsRequired()
            .HasMaxLength(5)
            .HasDefaultValue("en");

        builder.Property(q => q.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Generated");

        builder.Property(q => q.IncludesAudio)
            .HasDefaultValue(true);

        builder.Property(q => q.PlayCount)
            .HasDefaultValue(0);

        builder.Property(q => q.IsPublic)
            .HasDefaultValue(false);

        builder.Property(q => q.Tags)
            .HasMaxLength(500);

        builder.Property(q => q.AverageScore)
            .HasPrecision(5, 2); // 999.99 format

        // Indexes
        builder.HasIndex(q => q.UserId)
            .HasDatabaseName("IX_Quizzes_UserId");

        builder.HasIndex(q => new { q.Status, q.ExpiresAt })
            .HasDatabaseName("IX_Quizzes_Status_ExpiresAt");

        builder.HasIndex(q => new { q.IsPublic, q.Status })
            .HasDatabaseName("IX_Quizzes_IsPublic_Status");

        builder.HasIndex(q => q.CreatedAt)
            .HasDatabaseName("IX_Quizzes_CreatedAt");

        // Relationships
        builder.HasMany(q => q.Questions)
            .WithOne(qu => qu.Quiz)
            .HasForeignKey(qu => qu.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(q => q.Sessions)
            .WithOne(qs => qs.Quiz)
            .HasForeignKey(qs => qs.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(q => q.GenerationMetadata)
            .WithOne(gm => gm.Quiz)
            .HasForeignKey<QuizGenerationMetadata>(gm => gm.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        // User relationship is configured in UserConfiguration
    }
}