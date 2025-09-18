namespace VibeGuess.Spotify.Authentication.Models;

/// <summary>
/// Spotify OAuth 2.0 token response.
/// </summary>
public class SpotifyTokenResponse
{
    /// <summary>
    /// Access token for Spotify API requests.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token type (typically "Bearer").
    /// </summary>
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// Space-separated list of granted scopes.
    /// </summary>
    public string Scope { get; set; } = string.Empty;

    /// <summary>
    /// Token lifetime in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Refresh token for obtaining new access tokens.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}