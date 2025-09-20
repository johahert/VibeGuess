using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit;

namespace VibeGuess.Api.Tests.Contracts;

/// <summary>
/// Contract tests for GET /api/playback/devices endpoint.
/// These tests MUST FAIL initially (TDD RED phase) before implementation.
/// </summary>
public class PlaybackDevicesContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PlaybackDevicesContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetPlaybackDevices_WithValidAuthentication_ShouldReturn200WithDeviceList()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/playback/devices");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(result.TryGetProperty("devices", out var devicesProperty));
        Assert.Equal(JsonValueKind.Array, devicesProperty.ValueKind);
        
        // If there are devices, validate structure
        if (devicesProperty.GetArrayLength() > 0)
        {
            var firstDevice = devicesProperty[0];
            Assert.True(firstDevice.TryGetProperty("id", out _));
            Assert.True(firstDevice.TryGetProperty("name", out _));
            Assert.True(firstDevice.TryGetProperty("type", out _));
            Assert.True(firstDevice.TryGetProperty("isActive", out _));
            Assert.True(firstDevice.TryGetProperty("isPrivateSession", out _));
            Assert.True(firstDevice.TryGetProperty("isRestricted", out _));
            Assert.True(firstDevice.TryGetProperty("volumePercent", out _));
            Assert.True(firstDevice.TryGetProperty("supportsVolume", out _));
        }
    }

    [Fact]
    public async Task GetPlaybackDevices_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No authentication header

        // Act
        var response = await _client.GetAsync("/api/playback/devices");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("unauthorized", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task GetPlaybackDevices_WithExpiredToken_ShouldReturn401Unauthorized()
    {
        // Arrange
        var expiredToken = "Bearer expired.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "expired.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/playback/devices");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("unauthorized", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task GetPlaybackDevices_WithInvalidSpotifyToken_ShouldReturn403Forbidden()
    {
        // Arrange
        var validJwtButInvalidSpotify = "Bearer valid.jwt.invalid.spotify";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.invalid.spotify");

        // Act
        var response = await _client.GetAsync("/api/playback/devices");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("spotify", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task GetPlaybackDevices_WhenNoDevicesAvailable_ShouldReturnEmptyArray()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/playback/devices");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(result.TryGetProperty("devices", out var devicesProperty));
        Assert.Equal(JsonValueKind.Array, devicesProperty.ValueKind);
        // Should return empty array, not null
        Assert.True(devicesProperty.GetArrayLength() >= 0);
    }

    [Fact]
    public async Task GetPlaybackDevices_ShouldIncludeActiveDeviceFlag()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/playback/devices");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(result.TryGetProperty("activeDevice", out var activeDeviceProperty));
        
        // activeDevice should be null if no device is active, or contain device info
        if (activeDeviceProperty.ValueKind != JsonValueKind.Null)
        {
            Assert.True(activeDeviceProperty.TryGetProperty("id", out _));
            Assert.True(activeDeviceProperty.TryGetProperty("name", out _));
        }
    }

    [Fact]
    public async Task GetPlaybackDevices_ShouldFilterRestrictedDevices()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/playback/devices?includeRestricted=false");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(result.TryGetProperty("devices", out var devicesProperty));
        
        // All returned devices should not be restricted
        foreach (var device in devicesProperty.EnumerateArray())
        {
            Assert.True(device.TryGetProperty("isRestricted", out var isRestrictedProperty));
            Assert.False(isRestrictedProperty.GetBoolean());
        }
    }

    [Fact]
    public async Task GetPlaybackDevices_ShouldReturnCorrectContentType()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/playback/devices");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetPlaybackDevices_ResponseTime_ShouldBeFast()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/playback/devices");
        stopwatch.Stop();

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        // Response should be under 5 seconds (external API call to Spotify)
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Response took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
    }

    [Fact]
    public async Task GetPlaybackDevices_ShouldHandleSpotifyApiErrors()
    {
        // Arrange
        var validTokenButSpotifyDown = "Bearer valid.jwt.spotify.down";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.spotify.down");

        // Act
        var response = await _client.GetAsync("/api/playback/devices");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        // Should handle external service errors gracefully
        Assert.True(response.StatusCode == HttpStatusCode.ServiceUnavailable || 
                   response.StatusCode == HttpStatusCode.BadGateway);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("service", errorProperty.GetString().ToLower());
    }
}