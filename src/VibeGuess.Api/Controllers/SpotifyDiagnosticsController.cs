using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VibeGuess.Api.Services.Spotify;
using VibeGuess.Spotify.Authentication.Models;

namespace VibeGuess.Api.Controllers;

/// <summary>
/// Diagnostic controller for testing Spotify configuration
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SpotifyDiagnosticsController : ControllerBase
{
    private readonly SpotifyAuthenticationOptions _spotifyOptions;
    private readonly ISpotifyApiService _spotifyApiService;
    private readonly ILogger<SpotifyDiagnosticsController> _logger;

    public SpotifyDiagnosticsController(
        IOptions<SpotifyAuthenticationOptions> spotifyOptions,
        ISpotifyApiService spotifyApiService,
        ILogger<SpotifyDiagnosticsController> logger)
    {
        _spotifyOptions = spotifyOptions.Value;
        _spotifyApiService = spotifyApiService;
        _logger = logger;
    }

    /// <summary>
    /// Test endpoint to check Spotify configuration
    /// </summary>
    [HttpGet("config")]
    public ActionResult<object> GetSpotifyConfig()
    {
        return Ok(new
        {
            ClientIdExists = !string.IsNullOrEmpty(_spotifyOptions.ClientId),
            ClientId = string.IsNullOrEmpty(_spotifyOptions.ClientId) ? "MISSING" : $"{_spotifyOptions.ClientId[..8]}...",
            ClientSecretExists = !string.IsNullOrEmpty(_spotifyOptions.ClientSecret),
            ClientSecret = string.IsNullOrEmpty(_spotifyOptions.ClientSecret) ? "MISSING" : "***HIDDEN***",
            ApiBaseUrl = _spotifyOptions.ApiBaseUrl,
            RedirectUri = _spotifyOptions.RedirectUri
        });
    }

    /// <summary>
    /// Test endpoint to attempt Spotify client credentials authentication
    /// </summary>
    [HttpPost("test-auth")]
    public async Task<ActionResult<object>> TestSpotifyAuth()
    {
        try
        {
            if (string.IsNullOrEmpty(_spotifyOptions.ClientId) || string.IsNullOrEmpty(_spotifyOptions.ClientSecret))
            {
                return BadRequest(new { Error = "ClientId or ClientSecret not configured" });
            }

            using var httpClient = new HttpClient();
            
            var requestBody = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"}
            };

            var content = new FormUrlEncodedContent(requestBody);
            
            var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_spotifyOptions.ClientId}:{_spotifyOptions.ClientSecret}"));
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            _logger.LogInformation("Testing Spotify client credentials with ClientId: {ClientId}", _spotifyOptions.ClientId);

            var response = await httpClient.PostAsync("https://accounts.spotify.com/api/token", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return Ok(new
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = response.StatusCode,
                ResponseContent = responseContent,
                ClientIdUsed = _spotifyOptions.ClientId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Spotify authentication");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Test endpoint for Spotify authentication using the updated service
    /// </summary>
    [HttpGet("test-authentication")]
    public async Task<ActionResult<object>> TestAuthentication()
    {
        try
        {
            _logger.LogInformation("Testing Spotify authentication via SpotifyApiService");

            // Try to search for a popular track to test authentication
            var testTrack = await _spotifyApiService.SearchTrackAsync("Shape of You", "Ed Sheeran");

            if (testTrack != null)
            {
                return Ok(new
                {
                    Success = true,
                    Message = "Spotify authentication and API integration working",
                    TestTrack = new
                    {
                        testTrack.SpotifyTrackId,
                        testTrack.Name,
                        testTrack.ArtistName,
                        testTrack.AlbumName
                    }
                });
            }
            else
            {
                return Ok(new
                {
                    Success = false,
                    Message = "Authentication may have worked, but no track found or API error occurred"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Spotify authentication via service");
            return StatusCode(500, new { Success = false, Error = ex.Message });
        }
    }

    /// <summary>
    /// Test endpoint for searching specific tracks via the Spotify service
    /// </summary>
    [HttpGet("search-track")]
    public async Task<ActionResult<object>> SearchTrack([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { Error = "Query parameter is required" });
        }

        try
        {
            // Simple parsing - assumes format "Track Artist" or "Artist - Track"
            var parts = query.Contains(" - ") 
                ? query.Split(" - ", 2) 
                : query.Split(" ", 2);

            string trackName, artistName;
            
            if (parts.Length >= 2)
            {
                // If contains " - ", assume "Artist - Track", otherwise assume "Track Artist"
                if (query.Contains(" - "))
                {
                    artistName = parts[0].Trim();
                    trackName = parts[1].Trim();
                }
                else
                {
                    trackName = parts[0].Trim();
                    artistName = parts[1].Trim();
                }
            }
            else
            {
                return BadRequest(new { Error = "Query should contain both track and artist name, e.g. 'Shape of You Ed Sheeran' or 'Ed Sheeran - Shape of You'" });
            }

            _logger.LogInformation("Testing Spotify search for: {Artist} - {Track}", artistName, trackName);

            var track = await _spotifyApiService.SearchTrackAsync(trackName, artistName);

            if (track != null)
            {
                return Ok(new
                {
                    Success = true,
                    Found = true,
                    Query = query,
                    ParsedTrackName = trackName,
                    ParsedArtistName = artistName,
                    Result = new
                    {
                        track.SpotifyTrackId,
                        track.Name,
                        track.ArtistName,
                        track.AllArtists,
                        track.AlbumName,
                        track.DurationMs,
                        track.Popularity,
                        track.PreviewUrl,
                        track.SpotifyUrl,
                        track.AlbumImageUrl
                    }
                });
            }
            else
            {
                return Ok(new
                {
                    Success = true,
                    Found = false,
                    Query = query,
                    ParsedTrackName = trackName,
                    ParsedArtistName = artistName,
                    Message = "No track found matching the search criteria"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for track with query: {Query}", query);
            return StatusCode(500, new { Success = false, Error = ex.Message, Query = query });
        }
    }
}