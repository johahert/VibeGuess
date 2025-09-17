using System.ComponentModel.DataAnnotations;

namespace VibeGuess.Core.Entities;

/// <summary>
/// Represents Spotify track information used in quiz questions.
/// </summary>
public class Track : BaseEntity
{

    /// <summary>
    /// Spotify track ID.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SpotifyTrackId { get; set; } = string.Empty;

    /// <summary>
    /// Track name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Primary artist name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ArtistName { get; set; } = string.Empty;

    /// <summary>
    /// All artists on the track (comma-separated).
    /// </summary>
    [MaxLength(500)]
    public string AllArtists { get; set; } = string.Empty;

    /// <summary>
    /// Album name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string AlbumName { get; set; } = string.Empty;

    /// <summary>
    /// Album release date.
    /// </summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>
    /// Track duration in milliseconds.
    /// </summary>
    public int DurationMs { get; set; }

    /// <summary>
    /// Track popularity score from Spotify (0-100).
    /// </summary>
    [Range(0, 100)]
    public int Popularity { get; set; }

    /// <summary>
    /// Whether the track is explicit.
    /// </summary>
    public bool IsExplicit { get; set; }

    /// <summary>
    /// Preview URL for 30-second snippet (if available).
    /// </summary>
    [MaxLength(500)]
    public string? PreviewUrl { get; set; }

    /// <summary>
    /// Spotify external URL for the track.
    /// </summary>
    [MaxLength(500)]
    public string? SpotifyUrl { get; set; }

    /// <summary>
    /// Album cover image URL (640x640).
    /// </summary>
    [MaxLength(500)]
    public string? AlbumImageUrl { get; set; }

    /// <summary>
    /// Available markets (country codes, comma-separated).
    /// </summary>
    [MaxLength(1000)]
    public string? AvailableMarkets { get; set; }

    /// <summary>
    /// ISRC (International Standard Recording Code).
    /// </summary>
    [MaxLength(20)]
    public string? Isrc { get; set; }

    /// <summary>
    /// Audio features from Spotify (JSON string).
    /// </summary>
    public string? AudioFeatures { get; set; }



    // Navigation properties

    /// <summary>
    /// Questions that use this track.
    /// </summary>
    public ICollection<Question> Questions { get; set; } = new List<Question>();

    // Helper properties

    /// <summary>
    /// Track duration in a human-readable format (mm:ss).
    /// </summary>
    public string FormattedDuration
    {
        get
        {
            var duration = TimeSpan.FromMilliseconds(DurationMs);
            return $"{duration.Minutes:D2}:{duration.Seconds:D2}";
        }
    }

    /// <summary>
    /// Release year from the release date.
    /// </summary>
    public int? ReleaseYear => ReleaseDate?.Year;
}