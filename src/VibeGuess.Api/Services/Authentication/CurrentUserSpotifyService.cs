using System.Security.Claims;
using VibeGuess.Infrastructure.Repositories.Interfaces;
using VibeGuess.Spotify.Authentication.Services;

namespace VibeGuess.Api.Services.Authentication;

/// <summary>
/// Service for getting authenticated user's Spotify access tokens.
/// </summary>
public interface ICurrentUserSpotifyService
{
    /// <summary>
    /// Gets a valid Spotify access token for the currently authenticated user.
    /// Returns null if no user is authenticated or if tokens are invalid.
    /// </summary>
    Task<string?> GetCurrentUserSpotifyTokenAsync();
    
    /// <summary>
    /// Gets the current user's Spotify user ID.
    /// Returns null if no user is authenticated.
    /// </summary>
    string? GetCurrentUserSpotifyId();
}

/// <summary>
/// Implementation of service for getting authenticated user's Spotify access tokens.
/// </summary>
public class CurrentUserSpotifyService : ICurrentUserSpotifyService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISpotifyAuthenticationService _spotifyAuth;
    private readonly ILogger<CurrentUserSpotifyService> _logger;

    public CurrentUserSpotifyService(
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork,
        ISpotifyAuthenticationService spotifyAuth,
        ILogger<CurrentUserSpotifyService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _spotifyAuth = spotifyAuth;
        _logger = logger;
    }

    public async Task<string?> GetCurrentUserSpotifyTokenAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogDebug("No authenticated user found for Spotify token request");
                return null;
            }

            var accessToken = await _spotifyAuth.GetValidAccessTokenAsync(userId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("No valid Spotify access token found for user: {UserId}", userId);
                return null;
            }

            _logger.LogDebug("Successfully retrieved Spotify access token for user: {UserId}", userId);
            return accessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user's Spotify access token");
            return null;
        }
    }

    public string? GetCurrentUserSpotifyId()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            // Try to get Spotify user ID from JWT claims
            var spotifyUserIdClaim = httpContext.User.FindFirst("spotify_user_id");
            if (spotifyUserIdClaim != null)
            {
                return spotifyUserIdClaim.Value;
            }

            _logger.LogDebug("No Spotify user ID found in current user claims");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user's Spotify ID");
            return null;
        }
    }

    private Guid? GetCurrentUserId()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            // Try to get user ID from JWT claims
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            _logger.LogDebug("No valid user ID found in current user claims");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user ID");
            return null;
        }
    }
}