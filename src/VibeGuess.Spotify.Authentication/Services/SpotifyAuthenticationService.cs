using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VibeGuess.Core.Entities;
using VibeGuess.Infrastructure.Repositories.Interfaces;
using VibeGuess.Spotify.Authentication.Models;

namespace VibeGuess.Spotify.Authentication.Services;

/// <summary>
/// Implementation of Spotify OAuth 2.0 PKCE authentication service.
/// </summary>
public class SpotifyAuthenticationService : ISpotifyAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpotifyAuthenticationService> _logger;
    private readonly SpotifyAuthenticationOptions _options;
    private readonly IUnitOfWork _unitOfWork;

    public SpotifyAuthenticationService(
        HttpClient httpClient,
        ILogger<SpotifyAuthenticationService> logger,
        IOptions<SpotifyAuthenticationOptions> options,
        IUnitOfWork unitOfWork)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public Task<(PkceChallenge Challenge, string AuthorizationUrl)> CreateAuthorizationRequestAsync()
    {
        _logger.LogInformation("Creating Spotify authorization request");

        var challenge = PkceHelper.GenerateChallenge();
        var scopes = string.Join(" ", _options.Scopes);

        var authUrl = $"{_options.AuthorizationEndpoint}?" +
            $"client_id={Uri.EscapeDataString(_options.ClientId)}&" +
            $"response_type=code&" +
            $"redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}&" +
            $"code_challenge_method=S256&" +
            $"code_challenge={Uri.EscapeDataString(challenge.CodeChallenge)}&" +
            $"state={Uri.EscapeDataString(challenge.State)}&" +
            $"scope={Uri.EscapeDataString(scopes)}";

        _logger.LogInformation("Generated authorization URL with state: {State}", challenge.State);

        return Task.FromResult((challenge, authUrl));
    }

    /// <inheritdoc />
    public async Task<SpotifyTokenResponse> ExchangeCodeForTokensAsync(string authorizationCode, string codeVerifier, string state)
    {
        _logger.LogInformation("Exchanging authorization code for tokens with state: {State}", state);

        if (string.IsNullOrEmpty(authorizationCode))
            throw new ArgumentException("Authorization code is required", nameof(authorizationCode));
        if (string.IsNullOrEmpty(codeVerifier))
            throw new ArgumentException("Code verifier is required", nameof(codeVerifier));

        var requestBody = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("redirect_uri", _options.RedirectUri),
            new KeyValuePair<string, string>("client_id", _options.ClientId),
            new KeyValuePair<string, string>("code_verifier", codeVerifier)
        });

        try
        {
            var response = await _httpClient.PostAsync(_options.TokenEndpoint, requestBody);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to exchange code for tokens. Status: {Status}, Content: {Content}", 
                    response.StatusCode, content);
                throw new InvalidOperationException($"Failed to exchange authorization code: {response.StatusCode}");
            }

            var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (tokenResponse == null)
                throw new InvalidOperationException("Failed to deserialize token response");

            _logger.LogInformation("Successfully exchanged authorization code for tokens");
            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging authorization code for tokens");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SpotifyTokenResponse> RefreshAccessTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Refreshing Spotify access token");

        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentException("Refresh token is required", nameof(refreshToken));

        var requestBody = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
            new KeyValuePair<string, string>("client_id", _options.ClientId)
        });

        try
        {
            var response = await _httpClient.PostAsync(_options.TokenEndpoint, requestBody);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to refresh access token. Status: {Status}, Content: {Content}", 
                    response.StatusCode, content);
                throw new InvalidOperationException($"Failed to refresh access token: {response.StatusCode}");
            }

            var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (tokenResponse == null)
                throw new InvalidOperationException("Failed to deserialize refresh token response");

            // If no new refresh token is provided, use the existing one
            if (string.IsNullOrEmpty(tokenResponse.RefreshToken))
                tokenResponse.RefreshToken = refreshToken;

            _logger.LogInformation("Successfully refreshed access token");
            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing access token");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SpotifyUserProfile> GetUserProfileAsync(string accessToken)
    {
        _logger.LogInformation("Retrieving Spotify user profile");

        if (string.IsNullOrEmpty(accessToken))
            throw new ArgumentException("Access token is required", nameof(accessToken));

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"{_options.ApiBaseUrl}/me");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve user profile. Status: {Status}, Content: {Content}", 
                    response.StatusCode, content);
                throw new InvalidOperationException($"Failed to retrieve user profile: {response.StatusCode}");
            }

            var userProfile = JsonSerializer.Deserialize<SpotifyUserProfile>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (userProfile == null)
                throw new InvalidOperationException("Failed to deserialize user profile response");

            _logger.LogInformation("Successfully retrieved user profile for: {UserId}", userProfile.Id);
            return userProfile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            throw;
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    /// <inheritdoc />
    public async Task<SpotifyToken> StoreUserTokensAsync(Guid userId, SpotifyTokenResponse tokenResponse)
    {
        _logger.LogInformation("Storing Spotify tokens for user: {UserId}", userId);

        if (tokenResponse == null)
            throw new ArgumentNullException(nameof(tokenResponse));

        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            // Deactivate existing tokens for the user
            await _unitOfWork.SpotifyTokens.DeactivateUserTokensAsync(userId);

            // Create new token record
            var spotifyToken = new SpotifyToken
            {
                UserId = userId,
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                TokenType = tokenResponse.TokenType,
                Scope = tokenResponse.Scope,
                ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                IsActive = true
            };

            spotifyToken = await _unitOfWork.SpotifyTokens.AddAsync(spotifyToken);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Successfully stored tokens for user: {UserId}", userId);
            return spotifyToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing tokens for user: {UserId}", userId);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetValidAccessTokenAsync(Guid userId)
    {
        _logger.LogInformation("Getting valid access token for user: {UserId}", userId);

        try
        {
            var tokens = await _unitOfWork.SpotifyTokens.GetActiveTokensForUserAsync(userId);
            var activeToken = tokens.FirstOrDefault();

            if (activeToken == null)
            {
                _logger.LogWarning("No active tokens found for user: {UserId}", userId);
                return null;
            }

            // If token needs refresh
            if (activeToken.NeedsRefresh)
            {
                _logger.LogInformation("Token needs refresh for user: {UserId}", userId);
                
                try
                {
                    var refreshedTokens = await RefreshAccessTokenAsync(activeToken.RefreshToken);
                    await StoreUserTokensAsync(userId, refreshedTokens);
                    return refreshedTokens.AccessToken;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to refresh token for user: {UserId}", userId);
                    return null;
                }
            }

            return activeToken.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting valid access token for user: {UserId}", userId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task RevokeUserTokensAsync(Guid userId)
    {
        _logger.LogInformation("Revoking tokens for user: {UserId}", userId);

        try
        {
            await _unitOfWork.SpotifyTokens.DeactivateUserTokensAsync(userId);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Successfully revoked tokens for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking tokens for user: {UserId}", userId);
            throw;
        }
    }
}