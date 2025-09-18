using VibeGuess.Core.Entities;

namespace VibeGuess.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface for User entity operations.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user by their Spotify user ID.
    /// </summary>
    /// <param name="spotifyUserId">The Spotify user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user or null if not found</returns>
    Task<User?> GetBySpotifyUserIdAsync(string spotifyUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user or null if not found</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users by country.
    /// </summary>
    /// <param name="country">The country code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of users from the specified country</returns>
    Task<IEnumerable<User>> GetByCountryAsync(string country, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users with Spotify Premium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of users with Spotify Premium</returns>
    Task<IEnumerable<User>> GetPremiumUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user exists with the given Spotify user ID.
    /// </summary>
    /// <param name="spotifyUserId">The Spotify user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user exists, false otherwise</returns>
    Task<bool> ExistsBySpotifyUserIdAsync(string spotifyUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user with their settings included.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user with settings or null if not found</returns>
    Task<User?> GetWithSettingsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user with their Spotify tokens included.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user with tokens or null if not found</returns>
    Task<User?> GetWithTokensAsync(Guid id, CancellationToken cancellationToken = default);
}