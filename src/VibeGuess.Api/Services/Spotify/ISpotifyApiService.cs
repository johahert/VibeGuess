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
    /// Searches for tracks on Spotify using a single query string.
    /// </summary>
    /// <param name="query">Free-form query string (track, artist, etc.).</param>
    /// <param name="limit">Maximum number of tracks to return (1-10).</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching tracks, or null if the search could not be completed</returns>
    Task<IReadOnlyList<Track>?> SearchTracksAsync(string query, int limit = 10, CancellationToken cancellationToken = default);

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

    /// <summary>
    /// Start or resume playback on a Spotify device.
    /// Requires user authentication with playback scope.
    /// </summary>
    /// <param name="deviceId">Device ID to play on (optional - uses active device if not specified)</param>
    /// <param name="trackUris">Track URIs to play (optional)</param>
    /// <param name="contextUri">Context URI (playlist/album) to play (optional)</param>
    /// <param name="positionMs">Position to start playback at in milliseconds (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> StartPlaybackAsync(string? deviceId = null, string[]? trackUris = null, string? contextUri = null, int? positionMs = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pause playback on a Spotify device.
    /// Requires user authentication with playback scope.
    /// </summary>
    /// <param name="deviceId">Device ID to pause (optional - uses active device if not specified)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> PausePlaybackAsync(string? deviceId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current playback state for the user.
    /// Requires user authentication.
    /// </summary>
    /// <param name="market">Market for track relinking (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current playback state or null if not available</returns>
    Task<SpotifyPlaybackState?> GetCurrentPlaybackAsync(string? market = null, CancellationToken cancellationToken = default);
}