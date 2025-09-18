using VibeGuess.Core.Entities;

namespace VibeGuess.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface for Track entity operations.
/// </summary>
public interface ITrackRepository : IRepository<Track>
{
    /// <summary>
    /// Gets a track by its Spotify track ID.
    /// </summary>
    /// <param name="spotifyTrackId">The Spotify track ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The track or null if not found</returns>
    Task<Track?> GetBySpotifyTrackIdAsync(string spotifyTrackId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tracks by artist name.
    /// </summary>
    /// <param name="artistName">The artist name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of tracks by the artist</returns>
    Task<IEnumerable<Track>> GetByArtistAsync(string artistName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tracks by album name.
    /// </summary>
    /// <param name="albumName">The album name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of tracks from the album</returns>
    Task<IEnumerable<Track>> GetByAlbumAsync(string albumName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tracks by popularity range.
    /// </summary>
    /// <param name="minPopularity">Minimum popularity (0-100)</param>
    /// <param name="maxPopularity">Maximum popularity (0-100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of tracks within the popularity range</returns>
    Task<IEnumerable<Track>> GetByPopularityRangeAsync(int minPopularity, int maxPopularity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tracks by release year.
    /// </summary>
    /// <param name="year">The release year</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of tracks released in the specified year</returns>
    Task<IEnumerable<Track>> GetByReleaseYearAsync(int year, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tracks with preview URLs available.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of tracks with preview URLs</returns>
    Task<IEnumerable<Track>> GetWithPreviewUrlAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches tracks by name, artist, or album.
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching tracks</returns>
    Task<IEnumerable<Track>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets random tracks for quiz generation.
    /// </summary>
    /// <param name="count">Number of tracks to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of random tracks</returns>
    Task<IEnumerable<Track>> GetRandomTracksAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a track exists with the given Spotify track ID.
    /// </summary>
    /// <param name="spotifyTrackId">The Spotify track ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if track exists, false otherwise</returns>
    Task<bool> ExistsBySpotifyTrackIdAsync(string spotifyTrackId, CancellationToken cancellationToken = default);
}