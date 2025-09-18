using VibeGuess.Core.Entities;
using VibeGuess.Spotify.Authentication.Models;

namespace VibeGuess.Spotify.Authentication.Services;

/// <summary>
/// Service for handling Spotify OAuth 2.0 PKCE authentication flow.
/// </summary>
public interface ISpotifyAuthenticationService
{
    /// <summary>
    /// Generates a PKCE challenge and authorization URL for Spotify OAuth 2.0.
    /// </summary>
    /// <returns>PKCE challenge containing code verifier, challenge, state, and authorization URL</returns>
    Task<(PkceChallenge Challenge, string AuthorizationUrl)> CreateAuthorizationRequestAsync();

    /// <summary>
    /// Exchanges authorization code for access and refresh tokens.
    /// </summary>
    /// <param name="authorizationCode">Authorization code from Spotify callback</param>
    /// <param name="codeVerifier">Code verifier from the original PKCE challenge</param>
    /// <param name="state">State parameter for CSRF validation</param>
    /// <returns>Spotify token response</returns>
    Task<SpotifyTokenResponse> ExchangeCodeForTokensAsync(string authorizationCode, string codeVerifier, string state);

    /// <summary>
    /// Refreshes an expired access token using the refresh token.
    /// </summary>
    /// <param name="refreshToken">Refresh token from previous authentication</param>
    /// <returns>New token response</returns>
    Task<SpotifyTokenResponse> RefreshAccessTokenAsync(string refreshToken);

    /// <summary>
    /// Retrieves user profile information from Spotify API.
    /// </summary>
    /// <param name="accessToken">Valid Spotify access token</param>
    /// <returns>User profile information</returns>
    Task<SpotifyUserProfile> GetUserProfileAsync(string accessToken);

    /// <summary>
    /// Validates and stores Spotify tokens for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="tokenResponse">Token response from Spotify</param>
    /// <returns>Created or updated SpotifyToken entity</returns>
    Task<SpotifyToken> StoreUserTokensAsync(Guid userId, SpotifyTokenResponse tokenResponse);

    /// <summary>
    /// Gets valid access token for a user, refreshing if necessary.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Valid access token or null if no valid tokens available</returns>
    Task<string?> GetValidAccessTokenAsync(Guid userId);

    /// <summary>
    /// Revokes all Spotify tokens for a user (logout).
    /// </summary>
    /// <param name="userId">User ID</param>
    Task RevokeUserTokensAsync(Guid userId);
}