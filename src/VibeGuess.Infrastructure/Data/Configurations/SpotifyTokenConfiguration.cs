using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeGuess.Core.Entities;

namespace VibeGuess.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for SpotifyToken entity.
/// </summary>
public class SpotifyTokenConfiguration : IEntityTypeConfiguration<SpotifyToken>
{
    public void Configure(EntityTypeBuilder<SpotifyToken> builder)
    {
        // Table name
        builder.ToTable("SpotifyTokens");

        // Primary key
        builder.HasKey(st => st.Id);

        // Properties
        builder.Property(st => st.AccessToken)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(st => st.RefreshToken)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(st => st.TokenType)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Bearer");

        builder.Property(st => st.Scope)
            .HasMaxLength(1000);

        builder.Property(st => st.IsActive)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(st => st.UserId)
            .HasDatabaseName("IX_SpotifyTokens_UserId");

        builder.HasIndex(st => new { st.UserId, st.IsActive })
            .HasDatabaseName("IX_SpotifyTokens_UserId_IsActive");

        builder.HasIndex(st => st.ExpiresAt)
            .HasDatabaseName("IX_SpotifyTokens_ExpiresAt");

        // Relationships are configured in UserConfiguration
    }
}