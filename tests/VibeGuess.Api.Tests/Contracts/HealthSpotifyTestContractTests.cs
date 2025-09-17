using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace VibeGuess.Api.Tests.Contracts;

/// <summary>
/// Contract tests for POST /api/health/test/spotify endpoint.
/// These tests MUST FAIL initially (TDD RED phase) before implementation.
/// </summary>
public class HealthSpotifyTestContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HealthSpotifyTestContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task TestSpotifyConnection_WithValidAuthentication_ShouldReturn200WithConnectionStatus()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new { });
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/health/test/spotify", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var testResult = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(testResult.TryGetProperty("service", out var serviceProperty));
        Assert.Equal("spotify", serviceProperty.GetString());
        Assert.True(testResult.TryGetProperty("status", out var statusProperty));
        Assert.True(statusProperty.GetString() == "Connected" || 
                   statusProperty.GetString() == "Failed");
        Assert.True(testResult.TryGetProperty("timestamp", out _));
        Assert.True(testResult.TryGetProperty("duration", out _));
        Assert.True(testResult.TryGetProperty("details", out var detailsProperty));
        
        if (statusProperty.GetString() == "Connected")
        {
            Assert.True(detailsProperty.TryGetProperty("userProfile", out _));
            Assert.True(detailsProperty.TryGetProperty("scopes", out _));
        }
        else
        {
            Assert.True(detailsProperty.TryGetProperty("error", out _));
        }
    }

    [Fact]
    public async Task TestSpotifyConnection_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No authentication header
        var requestBody = JsonSerializer.Serialize(new { });
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/health/test/spotify", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("unauthorized", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task TestSpotifyConnection_WithInvalidSpotifyToken_ShouldReturn200WithFailedStatus()
    {
        // Arrange
        var validJwtInvalidSpotify = "Bearer valid.jwt.invalid.spotify";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.invalid.spotify");

        var requestBody = JsonSerializer.Serialize(new { });
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/health/test/spotify", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var testResult = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(testResult.TryGetProperty("status", out var statusProperty));
        Assert.Equal("Failed", statusProperty.GetString());
        Assert.True(testResult.TryGetProperty("details", out var detailsProperty));
        Assert.True(detailsProperty.TryGetProperty("error", out var errorProperty));
        Assert.Contains("token", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task TestSpotifyConnection_WithExpiredToken_ShouldReturn401Unauthorized()
    {
        // Arrange
        var expiredToken = "Bearer expired.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "expired.jwt.token");

        var requestBody = JsonSerializer.Serialize(new { });
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/health/test/spotify", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("expired", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task TestSpotifyConnection_WithAdminRole_ShouldIncludeExtendedDiagnostics()
    {
        // Arrange
        var adminToken = "Bearer valid.jwt.admin.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.admin.token");

        var requestBody = JsonSerializer.Serialize(new { includeExtendedDiagnostics = true });
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/health/test/spotify", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var testResult = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(testResult.TryGetProperty("details", out var detailsProperty));
        Assert.True(detailsProperty.TryGetProperty("apiVersion", out _));
        Assert.True(detailsProperty.TryGetProperty("rateLimitInfo", out _));
        Assert.True(detailsProperty.TryGetProperty("lastRequestTime", out _));
    }

    [Fact]
    public async Task TestSpotifyConnection_WithoutAdminRole_ShouldReturn403ForExtendedDiagnostics()
    {
        // Arrange
        var regularToken = "Bearer valid.jwt.regular.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.regular.token");

        var requestBody = JsonSerializer.Serialize(new { includeExtendedDiagnostics = true });
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/health/test/spotify", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("permission", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task TestSpotifyConnection_ShouldReturnCorrectContentType()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new { });
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/health/test/spotify", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task TestSpotifyConnection_ShouldBeFast()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new { });
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsync("/api/health/test/spotify", content);
        stopwatch.Stop();

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        // Spotify connection test should complete within 10 seconds
        Assert.True(stopwatch.ElapsedMilliseconds < 10000, 
            $"Spotify test took {stopwatch.ElapsedMilliseconds}ms, expected < 10000ms");
    }
}