namespace VibeGuess.Spotify.Authentication.Models;

/// <summary>
/// Represents an OAuth 2.0 PKCE challenge for Spotify authentication.
/// </summary>
public class PkceChallenge
{
    /// <summary>
    /// Code verifier - random string used for PKCE flow.
    /// </summary>
    public string CodeVerifier { get; set; } = string.Empty;

    /// <summary>
    /// Code challenge derived from code verifier using SHA256 and base64url encoding.
    /// </summary>
    public string CodeChallenge { get; set; } = string.Empty;

    /// <summary>
    /// State parameter for CSRF protection.
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// When the challenge was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the challenge expires (typically 10 minutes).
    /// </summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10);

    /// <summary>
    /// Whether the challenge has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}