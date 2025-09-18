namespace VibeGuess.Api.Models.Responses;

/// <summary>
/// Response model for Spotify login endpoint.
/// </summary>
public class SpotifyLoginResponse
{
    /// <summary>
    /// Authorization URL to redirect user to Spotify.
    /// </summary>
    public string AuthorizationUrl { get; set; } = string.Empty;

    /// <summary>
    /// Code verifier for PKCE flow (client should store this securely).
    /// </summary>
    public string CodeVerifier { get; set; } = string.Empty;

    /// <summary>
    /// State parameter for CSRF protection.
    /// </summary>
    public string State { get; set; } = string.Empty;
}