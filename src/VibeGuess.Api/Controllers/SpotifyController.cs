using Microsoft.AspNetCore.Mvc;
using VibeGuess.Api.Controllers;
using VibeGuess.Api.Services.Spotify;
using VibeGuess.Core.Entities;
using static VibeGuess.Api.Services.Spotify.SpotifyApiService;

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

    /// <summary>
    /// Get the user's available Spotify devices.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>List of available Spotify devices</returns>
    [HttpGet("devices")]
    public async Task<ActionResult<DevicesResponse>> GetDevices(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting user's Spotify devices");

            var devices = await _spotifyApiService.GetUserDevicesAsync(cancellationToken);

            if (devices == null)
            {
                return CreateErrorResponse(401, "authentication_required", "User must be authenticated with Spotify to access devices");
            }

            var devicesList = devices.ToList();
            var activeDevice = devicesList.FirstOrDefault(d => d.IsActive);

            return OkWithHeaders(new DevicesResponse
            {
                Devices = devicesList.Select(MapDeviceToResponse).ToList(),
                ActiveDevice = activeDevice != null ? MapDeviceToResponse(activeDevice) : null,
                DeviceCount = devicesList.Count,
                ActiveDeviceCount = devicesList.Count(d => d.IsActive)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user's Spotify devices");
            return CreateErrorResponse(500, "devices_error", "An error occurred while retrieving devices");
        }
    }

    private static DeviceDto MapDeviceToResponse(SpotifyDevice device)
    {
        return new DeviceDto
        {
            Id = device.Id,
            Name = device.Name,
            Type = device.Type,
            IsActive = device.IsActive,
            IsPrivateSession = device.IsPrivateSession,
            IsRestricted = device.IsRestricted,
            VolumePercent = device.VolumePercent,
            SupportsVolume = device.VolumePercent.HasValue
        };
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

/// <summary>
/// Response model for device operations.
/// </summary>
public class DevicesResponse
{
    /// <summary>
    /// List of available devices.
    /// </summary>
    public List<DeviceDto> Devices { get; set; } = new();

    /// <summary>
    /// The currently active device (if any).
    /// </summary>
    public DeviceDto? ActiveDevice { get; set; }

    /// <summary>
    /// Total number of devices found.
    /// </summary>
    public int DeviceCount { get; set; }

    /// <summary>
    /// Number of active devices.
    /// </summary>
    public int ActiveDeviceCount { get; set; }
}

/// <summary>
/// Data transfer object for device information.
/// </summary>
public class DeviceDto
{
    /// <summary>
    /// Spotify device ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Device name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Device type (Computer, Smartphone, etc.).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Whether this device is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Whether the device is in a private session.
    /// </summary>
    public bool IsPrivateSession { get; set; }

    /// <summary>
    /// Whether the device is restricted.
    /// </summary>
    public bool IsRestricted { get; set; }

    /// <summary>
    /// Current volume percentage (0-100).
    /// </summary>
    public int? VolumePercent { get; set; }

    /// <summary>
    /// Whether the device supports volume control.
    /// </summary>
    public bool SupportsVolume { get; set; }
}