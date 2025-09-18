namespace VibeGuess.Spotify.Authentication.Models;

/// <summary>
/// Configuration options for Spotify OAuth 2.0 authentication.
/// </summary>
public class SpotifyAuthenticationOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Spotify";

    /// <summary>
    /// Spotify application client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// OAuth 2.0 redirect URI configured in Spotify app settings.
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Spotify OAuth 2.0 authorization endpoint.
    /// </summary>
    public string AuthorizationEndpoint { get; set; } = "https://accounts.spotify.com/authorize";

    /// <summary>
    /// Spotify OAuth 2.0 token endpoint.
    /// </summary>
    public string TokenEndpoint { get; set; } = "https://accounts.spotify.com/api/token";

    /// <summary>
    /// Spotify Web API base URL.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.spotify.com/v1";

    /// <summary>
    /// Required Spotify OAuth scopes for the application.
    /// </summary>
    public string[] Scopes { get; set; } = {
        "user-read-private",
        "user-read-email",
        "user-top-read",
        "user-library-read",
        "playlist-read-private",
        "playlist-read-collaborative",
        "user-modify-playback-state",
        "user-read-playback-state",
        "user-read-currently-playing"
    };
}