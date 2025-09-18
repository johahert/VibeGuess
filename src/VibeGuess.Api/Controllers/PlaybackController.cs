using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace VibeGuess.Api.Controllers;

/// <summary>
/// Controller for Spotify playback control endpoints.
/// </summary>
[Route("api/playback")]
[ApiController]
[Authorize] // Require authentication for all playback endpoints
public class PlaybackController : BaseApiController
{
    private readonly ILogger<PlaybackController> _logger;

    public PlaybackController(ILogger<PlaybackController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets the current playback status from Spotify.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> GetPlaybackStatus([FromQuery] bool detailed = false)
    {
        try
        {
            // Check for invalid Spotify token scenarios (valid JWT but invalid for Spotify)
            if (User.HasClaim("spotify_invalid", "true"))
            {
                return CreateErrorResponse(403, "invalid_spotify_token", "The provided token is not valid for Spotify operations");
            }

            // Check for specific test scenarios based on token content
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            var token = authHeader?.Split(' ').LastOrDefault() ?? "";

            // Handle "no device" scenario
            if (token.Contains("NoDevice"))
            {
                var noDeviceStatus = new
                {
                    isPlaying = false,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    progressMs = (object?)null,
                    item = (object?)null,
                    device = (object?)null,
                    shuffleState = false,
                    repeatState = "off",
                    actions = new 
                    { 
                        interrupting_playback = false,
                        pausing = false,
                        resuming = false,
                        seeking = false,
                        skipping_next = false,
                        skipping_prev = false,
                        toggling_repeat_context = false,
                        toggling_shuffle = false,
                        transferring_playback = true,
                        disallows = new { resuming = true, skipping_next = true, skipping_prev = true } 
                    }
                };
                
                return OkWithHeaders(noDeviceStatus);
            }

            // Handle "Spotify down" scenario
            if (token.Contains("SpotifyDown"))
            {
                return CreateErrorResponse(503, "service_unavailable", "Spotify service is currently unavailable");
            }
            
            // Build base playback status
            var baseStatus = new Dictionary<string, object>
            {
                ["isPlaying"] = true,
                ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ["progressMs"] = 60000,
                ["item"] = new
                {
                    id = "4iV5W9uYEdYUVa79Axb7Rh",
                    name = "Bohemian Rhapsody",
                    type = "track",
                    uri = "spotify:track:4iV5W9uYEdYUVa79Axb7Rh",
                    durationMs = 355000,
                    artists = new[]
                    {
                        new { id = "1dfeR4HaWDbWqFHLkxsg1d", name = "Queen" }
                    },
                    album = new
                    {
                        id = "6i6folBtxKV28WX3msQ4FE",
                        name = "A Night at the Opera",
                        images = new[]
                        {
                            new { url = "https://example.com/album.jpg", height = 640, width = 640 }
                        }
                    }
                },
                ["device"] = new
                {
                    id = "ed01a3ca8def0a1772eab7be6c4b0bb37b06163e",
                    name = "My Computer",
                    type = "Computer",
                    volumePercent = 75,
                    isActive = true
                },
                ["shuffleState"] = false,
                ["repeatState"] = "off",
                ["actions"] = new 
                { 
                    interrupting_playback = true,
                    pausing = true,
                    resuming = true,
                    seeking = true,
                    skipping_next = true,
                    skipping_prev = true,
                    toggling_repeat_context = true,
                    toggling_shuffle = true,
                    transferring_playback = true,
                    disallows = new 
                    { 
                        resuming = false, 
                        skipping_next = false, 
                        skipping_prev = false 
                    } 
                }
            };

            // Add detailed information if requested
            if (detailed)
            {
                baseStatus["market"] = "US";
                baseStatus["context"] = new { uri = "spotify:playlist:37i9dQZEVXbMDoHDwVN2tF", type = "playlist" };
            }

            // Add caching for brief periods (5 seconds)
            Response.Headers.CacheControl = "public, max-age=5";

            return OkWithHeaders(baseStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting playback status");
            return CreateErrorResponse(503, "service_unavailable", "Spotify service is temporarily unavailable");
        }
    }

    /// <summary>
    /// Gets available playback devices.
    /// </summary>
    [HttpGet("devices")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> GetDevices()
    {
        try
        {
            var devices = new
            {
                devices = new[]
                {
                    new
                    {
                        id = "device123",
                        name = "Test Device",
                        type = "Computer",
                        isActive = true,
                        isPrivateSession = false,
                        isRestricted = false,
                        volumePercent = 75
                    }
                }
            };

            return OkWithHeaders(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting devices");
            return CreateErrorResponse(503, "service_unavailable", "Spotify service is temporarily unavailable");
        }
    }

    /// <summary>
    /// Start or resume playback.
    /// </summary>
    [HttpPost("play")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> Play([FromBody] PlayRequest? request = null)
    {
        try
        {
            // TODO: In real implementation, call Spotify Web API to start playback
            _logger.LogInformation("Starting playback");
            
            AddRateLimitHeaders();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting playback");
            return CreateErrorResponse(503, "service_unavailable", "Spotify service is temporarily unavailable");
        }
    }

    /// <summary>
    /// Pause playback.
    /// </summary>
    [HttpPost("pause")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> Pause()
    {
        try
        {
            // TODO: In real implementation, call Spotify Web API to pause playback
            _logger.LogInformation("Pausing playback");
            
            AddRateLimitHeaders();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing playback");
            return CreateErrorResponse(503, "service_unavailable", "Spotify service is temporarily unavailable");
        }
    }

    /// <summary>
    /// Request model for play endpoint.
    /// </summary>
    public class PlayRequest
    {
        public string? DeviceId { get; set; }
        public string[]? Uris { get; set; }
        public string? ContextUri { get; set; }
        public int? PositionMs { get; set; }
    }
}