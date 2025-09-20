using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit;

namespace VibeGuess.Api.Tests.Contracts;

/// <summary>
/// Contract tests for GET /api/playback/status endpoint.
/// These tests MUST FAIL initially (TDD RED phase) before implementation.
/// </summary>
public class PlaybackStatusContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PlaybackStatusContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetPlaybackStatus_WithValidAuthentication_ShouldReturn200WithCurrentStatus()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/playback/status");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var playbackStatus = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(playbackStatus.TryGetProperty("isPlaying", out _));
        Assert.True(playbackStatus.TryGetProperty("device", out var deviceProperty));
        
        if (deviceProperty.ValueKind != JsonValueKind.Null)
        {
            Assert.True(deviceProperty.TryGetProperty("id", out _));
            Assert.True(deviceProperty.TryGetProperty("name", out _));
            Assert.True(deviceProperty.TryGetProperty("type", out _));
            Assert.True(deviceProperty.TryGetProperty("volumePercent", out _));
        }
        
        Assert.True(playbackStatus.TryGetProperty("progressMs", out _));
        Assert.True(playbackStatus.TryGetProperty("shuffleState", out _));
        Assert.True(playbackStatus.TryGetProperty("repeatState", out _));
        Assert.True(playbackStatus.TryGetProperty("timestamp", out _));
        
        // Current track info (may be null if no track playing)
        Assert.True(playbackStatus.TryGetProperty("item", out var itemProperty));
        if (itemProperty.ValueKind != JsonValueKind.Null)
        {
            Assert.True(itemProperty.TryGetProperty("id", out _));
            Assert.True(itemProperty.TryGetProperty("name", out _));
            Assert.True(itemProperty.TryGetProperty("artists", out _));
            Assert.True(itemProperty.TryGetProperty("durationMs", out _));
            Assert.True(itemProperty.TryGetProperty("uri", out _));
        }
    }

    [Fact]
    public async Task GetPlaybackStatus_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No authentication header

        // Act
        var response = await _client.GetAsync("/api/playback/status");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("unauthorized", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task GetPlaybackStatus_WithExpiredToken_ShouldReturn401Unauthorized()
    {
        // Arrange
        var expiredToken = "Bearer expired.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "expired.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/playback/status");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("unauthorized", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task GetPlaybackStatus_WhenNoActiveDevice_ShouldReturn200WithNullDevice()
    {
        // Arrange
        var validTokenNoDevice = "Bearer valid.jwt.no.device";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.no.device");

        // Act
        var response = await _client.GetAsync("/api/playback/status");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var playbackStatus = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(playbackStatus.TryGetProperty("isPlaying", out var isPlayingProperty));
        Assert.False(isPlayingProperty.GetBoolean());
        Assert.True(playbackStatus.TryGetProperty("device", out var deviceProperty));
        Assert.Equal(JsonValueKind.Null, deviceProperty.ValueKind);
        Assert.True(playbackStatus.TryGetProperty("item", out var itemProperty));
        Assert.Equal(JsonValueKind.Null, itemProperty.ValueKind);
    }

    [Fact]
    public async Task GetPlaybackStatus_WithInvalidSpotifyToken_ShouldReturn403Forbidden()
    {
        // Arrange
        var validJwtButInvalidSpotify = "Bearer valid.jwt.invalid.spotify";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.invalid.spotify");

        // Act
        var response = await _client.GetAsync("/api/playback/status");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("spotify", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task GetPlaybackStatus_WithSpotifyApiError_ShouldReturn503ServiceUnavailable()
    {
        // Arrange
        var validTokenButSpotifyDown = "Bearer valid.jwt.spotify.down";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.spotify.down");

        // Act
        var response = await _client.GetAsync("/api/playback/status");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.True(response.StatusCode == HttpStatusCode.ServiceUnavailable || 
                   response.StatusCode == HttpStatusCode.BadGateway);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("service", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task GetPlaybackStatus_WithDetailedQuery_ShouldIncludeMarketAndContext()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/playback/status?includeContext=true&market=US");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var playbackStatus = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(playbackStatus.TryGetProperty("context", out var contextProperty));
        if (contextProperty.ValueKind != JsonValueKind.Null)
        {
            Assert.True(contextProperty.TryGetProperty("type", out _)); // playlist, album, artist
            Assert.True(contextProperty.TryGetProperty("uri", out _));
            Assert.True(contextProperty.TryGetProperty("href", out _));
        }
        
        Assert.True(playbackStatus.TryGetProperty("market", out var marketProperty));
        Assert.Equal("US", marketProperty.GetString());
    }

    [Fact]
    public async Task GetPlaybackStatus_ShouldIncludeActions()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/playback/status");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var playbackStatus = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(playbackStatus.TryGetProperty("actions", out var actionsProperty));
        Assert.True(actionsProperty.TryGetProperty("interrupting_playback", out _));
        Assert.True(actionsProperty.TryGetProperty("pausing", out _));
        Assert.True(actionsProperty.TryGetProperty("resuming", out _));
        Assert.True(actionsProperty.TryGetProperty("seeking", out _));
        Assert.True(actionsProperty.TryGetProperty("skipping_next", out _));
        Assert.True(actionsProperty.TryGetProperty("skipping_prev", out _));
        Assert.True(actionsProperty.TryGetProperty("toggling_repeat_context", out _));
        Assert.True(actionsProperty.TryGetProperty("toggling_shuffle", out _));
        Assert.True(actionsProperty.TryGetProperty("transferring_playback", out _));
    }

    [Fact]
    public async Task GetPlaybackStatus_ShouldReturnCorrectContentType()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/playback/status");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetPlaybackStatus_ResponseTime_ShouldBeFast()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/playback/status");
        stopwatch.Stop();

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        // Response should be under 3 seconds (external API call to Spotify)
        Assert.True(stopwatch.ElapsedMilliseconds < 3000, 
            $"Response took {stopwatch.ElapsedMilliseconds}ms, expected < 3000ms");
    }

    [Fact]
    public async Task GetPlaybackStatus_ShouldSupportCaching()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");
        _client.DefaultRequestHeaders.Add("If-None-Match", "\"playback-etag\"");

        // Act
        var response = await _client.GetAsync("/api/playback/status");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        // Should return 304 Not Modified if status hasn't changed, or include ETag if it has
        Assert.True(response.StatusCode == HttpStatusCode.NotModified || response.StatusCode == HttpStatusCode.OK);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            Assert.True(response.Headers.ETag != null || response.Headers.CacheControl != null);
        }
    }
}