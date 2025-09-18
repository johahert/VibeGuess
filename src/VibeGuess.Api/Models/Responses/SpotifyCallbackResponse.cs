namespace VibeGuess.Api.Models.Responses;

/// <summary>
/// Response model for Spotify callback endpoint.
/// </summary>
public class SpotifyCallbackResponse
{
    /// <summary>
    /// Access token for Spotify API requests.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token for obtaining new access tokens.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Token lifetime in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Token type (typically "Bearer").
    /// </summary>
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// User profile information.
    /// </summary>
    public UserProfileResponse User { get; set; } = new();
}

/// <summary>
/// User profile information from Spotify.
/// </summary>
public class UserProfileResponse
{
    /// <summary>
    /// Spotify user ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User's display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's country code.
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Whether user has Spotify Premium.
    /// </summary>
    public bool HasSpotifyPremium { get; set; }

    /// <summary>
    /// URL to user's profile image.
    /// </summary>
    public string? ProfileImageUrl { get; set; }
}