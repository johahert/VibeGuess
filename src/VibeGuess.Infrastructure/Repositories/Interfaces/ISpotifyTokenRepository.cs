using VibeGuess.Core.Entities;

namespace VibeGuess.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface for SpotifyToken entity operations.
/// </summary>
public interface ISpotifyTokenRepository : IRepository<SpotifyToken>
{
    /// <summary>
    /// Gets active Spotify tokens for a user.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active Spotify tokens for the user</returns>
    Task<IEnumerable<SpotifyToken>> GetActiveTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates all tokens for a user.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeactivateUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tokens that need refresh (expire within specified minutes).
    /// </summary>
    /// <param name="minutesBeforeExpiry">Minutes before expiry to consider for refresh</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of tokens that need refresh</returns>
    Task<IEnumerable<SpotifyToken>> GetTokensNeedingRefreshAsync(int minutesBeforeExpiry = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a Spotify token record by its refresh token value.
    /// </summary>
    /// <param name="refreshToken">The refresh token value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching SpotifyToken or null</returns>
    Task<SpotifyToken?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}