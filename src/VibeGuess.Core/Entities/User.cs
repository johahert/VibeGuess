using System.ComponentModel.DataAnnotations;

namespace VibeGuess.Core.Entities;

/// <summary>
/// Represents a user in the VibeGuess system.
/// Users authenticate via Spotify OAuth 2.0 PKCE and can create and play quizzes.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Spotify user ID - unique identifier from Spotify API.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string SpotifyUserId { get; set; } = string.Empty;

    /// <summary>
    /// User's display name from Spotify profile.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// User's email address from Spotify profile.
    /// </summary>
    [Required]
    [MaxLength(320)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user has Spotify Premium subscription.
    /// Premium users can control playback on Spotify devices.
    /// </summary>
    public bool HasSpotifyPremium { get; set; }

    /// <summary>
    /// User's country code (ISO 3166-1 alpha-2).
    /// </summary>
    [MaxLength(2)]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// URL to the user's profile image from Spotify.
    /// </summary>
    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// When the user last logged in.
    /// </summary>
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// User's role in the system (User, Admin).
    /// </summary>
    [MaxLength(50)]
    public string Role { get; set; } = "User";

    // Navigation properties

    /// <summary>
    /// User's preference settings.
    /// </summary>
    public UserSettings? Settings { get; set; }

    /// <summary>
    /// Quizzes created by this user.
    /// </summary>
    public ICollection<Quiz> CreatedQuizzes { get; set; } = new List<Quiz>();

    /// <summary>
    /// Quiz sessions played by this user.
    /// </summary>
    public ICollection<QuizSession> QuizSessions { get; set; } = new List<QuizSession>();

    /// <summary>
    /// Spotify authentication tokens for this user.
    /// </summary>
    public ICollection<SpotifyToken> SpotifyTokens { get; set; } = new List<SpotifyToken>();
}