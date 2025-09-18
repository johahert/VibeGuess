namespace VibeGuess.Spotify.Authentication.Models;

/// <summary>
/// Spotify user profile information from the API.
/// </summary>
public class SpotifyUserProfile
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
    /// Spotify subscription product type (free, premium, etc.).
    /// </summary>
    public string Product { get; set; } = string.Empty;

    /// <summary>
    /// Array of user profile images.
    /// </summary>
    public SpotifyImage[] Images { get; set; } = Array.Empty<SpotifyImage>();

    /// <summary>
    /// Whether the user has Spotify Premium.
    /// </summary>
    public bool HasPremium => Product?.Equals("premium", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// URL to the largest available profile image.
    /// </summary>
    public string? ProfileImageUrl => Images?.FirstOrDefault()?.Url;
}

/// <summary>
/// Represents a Spotify image.
/// </summary>
public class SpotifyImage
{
    /// <summary>
    /// Image URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Image height in pixels.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Image width in pixels.
    /// </summary>
    public int? Width { get; set; }
}