using System.ComponentModel.DataAnnotations;
using VibeGuess.Core.Interfaces;

namespace VibeGuess.Core.Entities;

/// <summary>
/// Represents Spotify authentication tokens for a user.
/// </summary>
public class SpotifyToken : ExpirableEntity, IUserOwned
{

    /// <summary>
    /// Spotify access token for API requests.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Spotify refresh token for obtaining new access tokens.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Token type (typically "Bearer").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Space-separated list of granted scopes.
    /// </summary>
    [MaxLength(1000)]
    public string Scope { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to the User entity.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Whether the token is currently valid/active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties

    /// <summary>
    /// The user these tokens belong to.
    /// </summary>
    public User User { get; set; } = null!;

    // Helper properties

    /// <summary>
    /// Whether the token needs to be refreshed (expires within 5 minutes).
    /// </summary>
    public bool NeedsRefresh => DateTime.UtcNow.AddMinutes(5) >= ExpiresAt;
}