using System.Security.Cryptography;
using System.Text;
using VibeGuess.Spotify.Authentication.Models;

namespace VibeGuess.Spotify.Authentication.Services;

/// <summary>
/// Helper service for generating PKCE (Proof Key for Code Exchange) challenges.
/// </summary>
public class PkceHelper
{
    /// <summary>
    /// Generates a new PKCE challenge with code verifier, code challenge, and state.
    /// </summary>
    /// <returns>PKCE challenge</returns>
    public static PkceChallenge GenerateChallenge()
    {
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        var state = GenerateState();

        return new PkceChallenge
        {
            CodeVerifier = codeVerifier,
            CodeChallenge = codeChallenge,
            State = state
        };
    }

    /// <summary>
    /// Generates a cryptographically secure random code verifier (43-128 characters).
    /// </summary>
    /// <returns>Base64URL-encoded code verifier</returns>
    private static string GenerateCodeVerifier()
    {
        var bytes = new byte[96]; // 96 bytes = 128 base64url characters
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Base64UrlEncode(bytes);
    }

    /// <summary>
    /// Generates code challenge from code verifier using SHA256.
    /// </summary>
    /// <param name="codeVerifier">Code verifier</param>
    /// <returns>Base64URL-encoded code challenge</returns>
    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var bytes = Encoding.UTF8.GetBytes(codeVerifier);
        var hash = SHA256.HashData(bytes);
        return Base64UrlEncode(hash);
    }

    /// <summary>
    /// Generates a random state parameter for CSRF protection.
    /// </summary>
    /// <returns>Base64URL-encoded state</returns>
    private static string GenerateState()
    {
        var bytes = new byte[32]; // 32 bytes = 43 base64url characters
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Base64UrlEncode(bytes);
    }

    /// <summary>
    /// Encodes bytes as base64url (RFC 4648 Section 5).
    /// </summary>
    /// <param name="bytes">Bytes to encode</param>
    /// <returns>Base64URL-encoded string</returns>
    private static string Base64UrlEncode(byte[] bytes)
    {
        var base64 = Convert.ToBase64String(bytes);
        return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}