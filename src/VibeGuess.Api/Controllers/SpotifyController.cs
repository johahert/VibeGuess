using Microsoft.AspNetCore.Mvc;
using VibeGuess.Api.Controllers;
using VibeGuess.Api.Services.Spotify;
using VibeGuess.Core.Entities;

namespace VibeGuess.Api.Controllers;

/// <summary>
/// Controller for Spotify API operations including track search and retrieval.
/// </summary>
[Route("api/[controller]")]
public class SpotifyController : BaseApiController
{
    private readonly ISpotifyApiService _spotifyApiService;
    private readonly ILogger<SpotifyController> _logger;

    public SpotifyController(
        ISpotifyApiService spotifyApiService,
        ILogger<SpotifyController> logger)
    {
        _spotifyApiService = spotifyApiService;
        _logger = logger;
    }

    /// <summary>
    /// Search for a track on Spotify by track name and artist name.
    /// </summary>
    /// <param name="trackName">The name of the track to search for</param>
    /// <param name="artistName">The name of the artist to search for</param>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>The found track or null if not found</returns>
    [HttpGet("search")]
    public async Task<ActionResult<TrackSearchResponse>> SearchTrack(
        [FromQuery] string trackName,
        [FromQuery] string artistName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(trackName))
        {
            return BadRequestWithError("Track name is required");
        }

        if (string.IsNullOrWhiteSpace(artistName))
        {
            return BadRequestWithError("Artist name is required");
        }

        try
        {
            _logger.LogInformation("Searching for track: {Artist} - {Track}", artistName, trackName);

            var track = await _spotifyApiService.SearchTrackAsync(trackName, artistName, cancellationToken);

            if (track == null)
            {
                return OkWithHeaders(new TrackSearchResponse
                {
                    Found = false,
                    Message = $"No track found for '{artistName} - {trackName}'"
                });
            }

            return OkWithHeaders(new TrackSearchResponse
            {
                Found = true,
                Track = MapTrackToResponse(track),
                Message = "Track found successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for track: {Artist} - {Track}", artistName, trackName);
            return CreateErrorResponse(500, "search_error", "An error occurred while searching for the track");
        }
    }

    /// <summary>
    /// Get a specific track by its Spotify track ID.
    /// </summary>
    /// <param name="spotifyTrackId">The Spotify track ID</param>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>The track details or null if not found</returns>
    [HttpGet("track/{spotifyTrackId}")]
    public async Task<ActionResult<TrackResponse>> GetTrack(
        string spotifyTrackId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(spotifyTrackId))
        {
            return BadRequestWithError("Spotify track ID is required");
        }

        try
        {
            _logger.LogInformation("Getting track details for Spotify ID: {SpotifyTrackId}", spotifyTrackId);

            var track = await _spotifyApiService.GetTrackAsync(spotifyTrackId, cancellationToken);

            if (track == null)
            {
                return NotFound(new { Message = $"Track with Spotify ID '{spotifyTrackId}' not found" });
            }

            return OkWithHeaders(new TrackResponse
            {
                Track = MapTrackToResponse(track)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting track: {SpotifyTrackId}", spotifyTrackId);
            return CreateErrorResponse(500, "track_error", "An error occurred while retrieving the track");
        }
    }

    private static TrackDto MapTrackToResponse(Track track)
    {
        return new TrackDto
        {
            Id = track.Id,
            SpotifyTrackId = track.SpotifyTrackId,
            Name = track.Name,
            ArtistName = track.ArtistName,
            AllArtists = track.AllArtists,
            AlbumName = track.AlbumName,
            DurationMs = track.DurationMs,
            Popularity = track.Popularity,
            IsExplicit = track.IsExplicit,
            PreviewUrl = track.PreviewUrl,
            SpotifyUrl = track.SpotifyUrl,
            AlbumImageUrl = track.AlbumImageUrl,
            ReleaseDate = track.ReleaseDate,
            CreatedAt = track.CreatedAt
        };
    }
}

/// <summary>
/// Response model for track search operations.
/// </summary>
public class TrackSearchResponse
{
    /// <summary>
    /// Indicates whether a track was found.
    /// </summary>
    public bool Found { get; set; }

    /// <summary>
    /// The found track (if any).
    /// </summary>
    public TrackDto? Track { get; set; }

    /// <summary>
    /// Response message providing additional context.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Response model for individual track retrieval.
/// </summary>
public class TrackResponse
{
    /// <summary>
    /// The track details.
    /// </summary>
    public TrackDto Track { get; set; } = null!;
}

/// <summary>
/// Data transfer object for track information.
/// </summary>
public class TrackDto
{
    /// <summary>
    /// Internal track ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Spotify track ID.
    /// </summary>
    public string SpotifyTrackId { get; set; } = string.Empty;

    /// <summary>
    /// Track name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Primary artist name.
    /// </summary>
    public string ArtistName { get; set; } = string.Empty;

    /// <summary>
    /// All artists (comma-separated).
    /// </summary>
    public string AllArtists { get; set; } = string.Empty;

    /// <summary>
    /// Album name.
    /// </summary>
    public string AlbumName { get; set; } = string.Empty;

    /// <summary>
    /// Track duration in milliseconds.
    /// </summary>
    public int DurationMs { get; set; }

    /// <summary>
    /// Spotify popularity score (0-100).
    /// </summary>
    public int Popularity { get; set; }

    /// <summary>
    /// Whether the track has explicit lyrics.
    /// </summary>
    public bool IsExplicit { get; set; }

    /// <summary>
    /// URL to 30-second preview (if available).
    /// </summary>
    public string? PreviewUrl { get; set; }

    /// <summary>
    /// Spotify URL for the track.
    /// </summary>
    public string? SpotifyUrl { get; set; }

    /// <summary>
    /// URL to album cover image.
    /// </summary>
    public string? AlbumImageUrl { get; set; }

    /// <summary>
    /// Release date of the album.
    /// </summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>
    /// When this track record was created in our system.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}