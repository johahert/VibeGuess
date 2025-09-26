using VibeGuess.Core.Entities;

namespace VibeGuess.Api.Services.Spotify;

/// <summary>
/// Interface for Spotify Web API service operations.
/// </summary>
public interface ISpotifyApiService
{
    /// <summary>
    /// Searches for tracks on Spotify by track name and artist.
    /// </summary>
    /// <param name="trackName">The track name to search for</param>
    /// <param name="artistName">The artist name to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The first matching track or null if not found</returns>
    Task<Track?> SearchTrackAsync(string trackName, string artistName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets track details by Spotify track ID.
    /// </summary>
    /// <param name="spotifyTrackId">The Spotify track ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Track details or null if not found</returns>
    Task<Track?> GetTrackAsync(string spotifyTrackId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user's available Spotify devices.
    /// Requires user authentication - will not work with client credentials only.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available Spotify devices or null if user not authenticated</returns>
    Task<IEnumerable<SpotifyDevice>?> GetUserDevicesAsync(CancellationToken cancellationToken = default);
}