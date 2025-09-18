using Microsoft.EntityFrameworkCore;
using VibeGuess.Core.Entities;
using VibeGuess.Infrastructure.Data;
using VibeGuess.Infrastructure.Repositories.Interfaces;

namespace VibeGuess.Infrastructure.Repositories.Implementations;

/// <summary>
/// Repository implementation for Track entity operations.
/// </summary>
public class TrackRepository : Repository<Track>, ITrackRepository
{
    public TrackRepository(VibeGuessDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<Track?> GetBySpotifyTrackIdAsync(string spotifyTrackId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(spotifyTrackId);

        return await _dbSet
            .FirstOrDefaultAsync(t => t.SpotifyTrackId == spotifyTrackId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Track>> GetByArtistAsync(string artistName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artistName);

        return await _dbSet
            .Where(t => t.ArtistName.ToLower().Contains(artistName.ToLower()) ||
                       t.AllArtists.ToLower().Contains(artistName.ToLower()))
            .OrderByDescending(t => t.Popularity)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Track>> GetByAlbumAsync(string albumName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(albumName);

        return await _dbSet
            .Where(t => t.AlbumName.ToLower().Contains(albumName.ToLower()))
            .OrderByDescending(t => t.Popularity)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Track>> GetByPopularityRangeAsync(int minPopularity, int maxPopularity, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.Popularity >= minPopularity && t.Popularity <= maxPopularity)
            .OrderByDescending(t => t.Popularity)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Track>> GetByReleaseYearAsync(int year, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.ReleaseDate.HasValue && t.ReleaseDate.Value.Year == year)
            .OrderByDescending(t => t.Popularity)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Track>> GetWithPreviewUrlAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => !string.IsNullOrEmpty(t.PreviewUrl))
            .OrderByDescending(t => t.Popularity)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Track>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchTerm);

        var term = searchTerm.ToLower();
        return await _dbSet
            .Where(t => t.Name.ToLower().Contains(term) ||
                       t.ArtistName.ToLower().Contains(term) ||
                       t.AlbumName.ToLower().Contains(term) ||
                       t.AllArtists.ToLower().Contains(term))
            .OrderByDescending(t => t.Popularity)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Track>> GetRandomTracksAsync(int count, CancellationToken cancellationToken = default)
    {
        // Note: This is a simplified random selection. For better performance with large datasets,
        // consider using more sophisticated random sampling techniques.
        var totalTracks = await CountAsync(cancellationToken);
        var random = new Random();
        var skipCount = Math.Max(0, random.Next(0, totalTracks - count));

        return await _dbSet
            .Skip(skipCount)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsBySpotifyTrackIdAsync(string spotifyTrackId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(spotifyTrackId);

        return await _dbSet
            .AnyAsync(t => t.SpotifyTrackId == spotifyTrackId, cancellationToken);
    }
}