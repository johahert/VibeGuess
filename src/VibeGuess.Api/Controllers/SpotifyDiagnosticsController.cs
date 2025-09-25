using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
    private readonly ILogger<SpotifyDiagnosticsController> _logger;

    public SpotifyDiagnosticsController(
        IOptions<SpotifyAuthenticationOptions> spotifyOptions,
        ILogger<SpotifyDiagnosticsController> logger)
    {
        _spotifyOptions = spotifyOptions.Value;
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
}