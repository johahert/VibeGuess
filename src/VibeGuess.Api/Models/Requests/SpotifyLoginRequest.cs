namespace VibeGuess.Api.Models.Requests;

/// <summary>
/// Request model for Spotify login endpoint.
/// </summary>
public class SpotifyLoginRequest
{
    /// <summary>
    /// The redirect URI where Spotify will send the authorization code.
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Optional state parameter for CSRF protection.
    /// </summary>
    public string? State { get; set; }
}