namespace VibeGuess.Api.Models.Requests;

/// <summary>
/// Request model for Spotify OAuth callback endpoint.
/// </summary>
public class SpotifyCallbackRequest
{
    /// <summary>
    /// Authorization code received from Spotify.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Code verifier from the original PKCE challenge.
    /// </summary>
    public string CodeVerifier { get; set; } = string.Empty;

    /// <summary>
    /// The redirect URI used in the authorization request.
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// State parameter for CSRF validation.
    /// </summary>
    public string? State { get; set; }
}