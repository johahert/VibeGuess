using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace VibeGuess.Api.Tests.Contracts;

/// <summary>
/// Contract tests for POST /api/playback/play endpoint.
/// These tests MUST FAIL initially (TDD RED phase) before implementation.
/// </summary>
public class PlaybackPlayContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PlaybackPlayContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task PlayTrack_WithValidRequest_ShouldReturn200WithPlaybackStatus()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            trackUri = "spotify:track:4iV5W9uYEdYUVa79Axb7Rh",
            deviceId = "spotify-device-id",
            positionMs = 0,
            volume = 75
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/play", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var playbackStatus = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(playbackStatus.TryGetProperty("isPlaying", out var isPlayingProperty));
        Assert.True(isPlayingProperty.GetBoolean());
        Assert.True(playbackStatus.TryGetProperty("trackUri", out var trackUriProperty));
        Assert.Equal("spotify:track:4iV5W9uYEdYUVa79Axb7Rh", trackUriProperty.GetString());
        Assert.True(playbackStatus.TryGetProperty("deviceId", out var deviceIdProperty));
        Assert.Equal("spotify-device-id", deviceIdProperty.GetString());
        Assert.True(playbackStatus.TryGetProperty("progressMs", out _));
        Assert.True(playbackStatus.TryGetProperty("volume", out var volumeProperty));
        Assert.Equal(75, volumeProperty.GetInt32());
    }

    [Fact]
    public async Task PlayTrack_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No authentication header
        var requestBody = JsonSerializer.Serialize(new
        {
            trackUri = "spotify:track:4iV5W9uYEdYUVa79Axb7Rh",
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/play", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("unauthorized", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task PlayTrack_WithInvalidTrackUri_ShouldReturn400BadRequest()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            trackUri = "invalid-track-uri",
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/play", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("track", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task PlayTrack_WithInvalidDeviceId_ShouldReturn400BadRequest()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            trackUri = "spotify:track:4iV5W9uYEdYUVa79Axb7Rh",
            deviceId = "non-existent-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/play", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("device", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task PlayTrack_WithMissingRequiredFields_ShouldReturn400BadRequest()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            // Missing trackUri and deviceId
            volume = 50
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/play", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.True(errorProperty.GetString().ToLower().Contains("required") ||
                   errorProperty.GetString().ToLower().Contains("trackuri") ||
                   errorProperty.GetString().ToLower().Contains("deviceid"));
    }

    [Fact]
    public async Task PlayTrack_WithInvalidVolumeRange_ShouldReturn400BadRequest()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            trackUri = "spotify:track:4iV5W9uYEdYUVa79Axb7Rh",
            deviceId = "spotify-device-id",
            volume = 150 // Invalid: should be 0-100
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/play", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("volume", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task PlayTrack_WithInvalidPositionMs_ShouldReturn400BadRequest()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            trackUri = "spotify:track:4iV5W9uYEdYUVa79Axb7Rh",
            deviceId = "spotify-device-id",
            positionMs = -1000 // Invalid: should be >= 0
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/play", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("position", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task PlayTrack_WithSpotifyApiError_ShouldReturn503ServiceUnavailable()
    {
        // Arrange
        var validTokenButSpotifyDown = "Bearer valid.jwt.spotify.down";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.spotify.down");

        var requestBody = JsonSerializer.Serialize(new
        {
            trackUri = "spotify:track:4iV5W9uYEdYUVa79Axb7Rh",
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/play", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.True(response.StatusCode == HttpStatusCode.ServiceUnavailable || 
                   response.StatusCode == HttpStatusCode.BadGateway);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("service", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task PlayTrack_WithRestrictedTrack_ShouldReturn403Forbidden()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            trackUri = "spotify:track:restricted-track",
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/play", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.True(errorProperty.GetString().ToLower().Contains("restricted") ||
                   errorProperty.GetString().ToLower().Contains("premium") ||
                   errorProperty.GetString().ToLower().Contains("market"));
    }

    [Fact]
    public async Task PlayTrack_ShouldReturnCorrectContentType()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            trackUri = "spotify:track:4iV5W9uYEdYUVa79Axb7Rh",
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/playback/play", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task PlayTrack_ResponseTime_ShouldBeFast()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            trackUri = "spotify:track:4iV5W9uYEdYUVa79Axb7Rh",
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsync("/api/playback/play", content);
        stopwatch.Stop();

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        // Response should be under 3 seconds (external API call to Spotify)
        Assert.True(stopwatch.ElapsedMilliseconds < 3000, 
            $"Response took {stopwatch.ElapsedMilliseconds}ms, expected < 3000ms");
    }
}