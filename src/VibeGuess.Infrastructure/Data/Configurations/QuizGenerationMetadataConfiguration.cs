using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeGuess.Core.Entities;

namespace VibeGuess.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for QuizGenerationMetadata entity.
/// </summary>
public class QuizGenerationMetadataConfiguration : IEntityTypeConfiguration<QuizGenerationMetadata>
{
    public void Configure(EntityTypeBuilder<QuizGenerationMetadata> builder)
    {
        // Table name
        builder.ToTable("QuizGenerationMetadata");

        // Primary key
        builder.HasKey(gm => gm.Id);

        // Properties
        builder.Property(gm => gm.AiModel)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(gm => gm.AiModelVersion)
            .HasMaxLength(50);

        builder.Property(gm => gm.Warnings)
            .HasMaxLength(2000);

        builder.Property(gm => gm.RawPrompt)
            .HasMaxLength(4000);

        builder.Property(gm => gm.RawResponse)
            .HasColumnType("nvarchar(max)"); // Can be very large

        builder.Property(gm => gm.EstimatedCostCents)
            .HasPrecision(10, 4); // Up to $999,999.9999

        // Indexes
        builder.HasIndex(gm => gm.QuizId)
            .IsUnique()
            .HasDatabaseName("IX_QuizGenerationMetadata_QuizId");

        builder.HasIndex(gm => gm.AiModel)
            .HasDatabaseName("IX_QuizGenerationMetadata_AiModel");

        builder.HasIndex(gm => gm.CreatedAt)
            .HasDatabaseName("IX_QuizGenerationMetadata_CreatedAt");

        // Relationships are configured in QuizConfiguration
    }
}