using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace VibeGuess.Api.Tests.Contracts;

/// <summary>
/// Contract tests for POST /api/playback/pause endpoint.
/// These tests MUST FAIL initially (TDD RED phase) before implementation.
/// </summary>
public class PlaybackPauseContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PlaybackPauseContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task PausePlayback_WithValidRequest_ShouldReturn200WithPausedStatus()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/pause", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var playbackStatus = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(playbackStatus.TryGetProperty("isPlaying", out var isPlayingProperty));
        Assert.False(isPlayingProperty.GetBoolean());
        Assert.True(playbackStatus.TryGetProperty("deviceId", out var deviceIdProperty));
        Assert.Equal("spotify-device-id", deviceIdProperty.GetString());
        Assert.True(playbackStatus.TryGetProperty("progressMs", out _));
        Assert.True(playbackStatus.TryGetProperty("pausedAt", out _));
    }

    [Fact]
    public async Task PausePlayback_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No authentication header
        var requestBody = JsonSerializer.Serialize(new
        {
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/pause", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("unauthorized", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task PausePlayback_WithoutActivePlayback_ShouldReturn400BadRequest()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            deviceId = "spotify-device-id-no-playback"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/pause", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("no active playback", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task PausePlayback_WithInvalidDeviceId_ShouldReturn400BadRequest()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            deviceId = "non-existent-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/pause", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("device", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task PausePlayback_WithMissingDeviceId_ShouldReturn400BadRequest()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            // Missing required deviceId
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/pause", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("deviceid", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task PausePlayback_WhenAlreadyPaused_ShouldReturn200WithPausedStatus()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            deviceId = "spotify-device-id-already-paused"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/pause", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var playbackStatus = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(playbackStatus.TryGetProperty("isPlaying", out var isPlayingProperty));
        Assert.False(isPlayingProperty.GetBoolean());
        Assert.True(playbackStatus.TryGetProperty("message", out var messageProperty));
        Assert.Contains("already paused", messageProperty.GetString().ToLower());
    }

    [Fact]
    public async Task PausePlayback_WithSpotifyApiError_ShouldReturn503ServiceUnavailable()
    {
        // Arrange
        var validTokenButSpotifyDown = "Bearer valid.jwt.spotify.down";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.spotify.down");

        var requestBody = JsonSerializer.Serialize(new
        {
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/pause", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.True(response.StatusCode == HttpStatusCode.ServiceUnavailable || 
                   response.StatusCode == HttpStatusCode.BadGateway);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("service", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task PausePlayback_WithInsufficientPermissions_ShouldReturn403Forbidden()
    {
        // Arrange
        var tokenWithoutPlaybackScope = "Bearer valid.jwt.no.playback.scope";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.no.playback.scope");

        var requestBody = JsonSerializer.Serialize(new
        {
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/pause", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.True(errorProperty.GetString().ToLower().Contains("permission") ||
                   errorProperty.GetString().ToLower().Contains("scope") ||
                   errorProperty.GetString().ToLower().Contains("premium"));
    }

    [Fact]
    public async Task PausePlayback_ShouldReturnCorrectContentType()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/pause", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task PausePlayback_ResponseTime_ShouldBeFast()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsync("/api/playback/pause", content);
        stopwatch.Stop();

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        // Response should be under 2 seconds (external API call to Spotify)
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, 
            $"Response took {stopwatch.ElapsedMilliseconds}ms, expected < 2000ms");
    }
}