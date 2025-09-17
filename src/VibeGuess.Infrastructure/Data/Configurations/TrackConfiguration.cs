using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeGuess.Core.Entities;

namespace VibeGuess.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Track entity.
/// </summary>
public class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        // Table name
        builder.ToTable("Tracks");

        // Primary key
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.SpotifyTrackId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.ArtistName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.AllArtists)
            .HasMaxLength(500);

        builder.Property(t => t.AlbumName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.PreviewUrl)
            .HasMaxLength(500);

        builder.Property(t => t.SpotifyUrl)
            .HasMaxLength(500);

        builder.Property(t => t.AlbumImageUrl)
            .HasMaxLength(500);

        builder.Property(t => t.AvailableMarkets)
            .HasMaxLength(1000);

        builder.Property(t => t.Isrc)
            .HasMaxLength(20);

        builder.Property(t => t.AudioFeatures)
            .HasColumnType("nvarchar(max)"); // JSON data

        // Indexes
        builder.HasIndex(t => t.SpotifyTrackId)
            .IsUnique()
            .HasDatabaseName("IX_Tracks_SpotifyTrackId");

        builder.HasIndex(t => t.ArtistName)
            .HasDatabaseName("IX_Tracks_ArtistName");

        builder.HasIndex(t => t.AlbumName)
            .HasDatabaseName("IX_Tracks_AlbumName");

        builder.HasIndex(t => t.ReleaseDate)
            .HasDatabaseName("IX_Tracks_ReleaseDate");

        builder.HasIndex(t => t.Popularity)
            .HasDatabaseName("IX_Tracks_Popularity");

        // Relationships are configured in QuestionConfiguration
    }
}