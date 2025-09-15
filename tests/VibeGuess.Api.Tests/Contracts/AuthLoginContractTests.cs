using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace VibeGuess.Api.Tests.Contracts;

public class AuthLoginContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthLoginContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task POST_AuthSpotifyLogin_WithValidRequest_Returns200WithAuthUrl()
    {
        // Arrange - Create request per auth-api.md contract
        var loginRequest = new
        {
            redirectUri = "https://test-app.com/callback",
            state = "test-state-parameter"
        };
        
        var requestJson = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act - POST to /api/auth/spotify/login endpoint
        var response = await _client.PostAsync("/api/auth/spotify/login", content);

        // Assert - Validate contract response per auth-api.md
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        // Validate required response fields per contract
        Assert.True(responseJson.TryGetProperty("authorizationUrl", out var authUrl));
        Assert.True(responseJson.TryGetProperty("codeVerifier", out var codeVerifier));
        Assert.True(responseJson.TryGetProperty("state", out var state));
        
        // Validate response field types and formats
        Assert.True(authUrl.GetString()?.StartsWith("https://accounts.spotify.com/authorize"));
        Assert.NotEmpty(codeVerifier.GetString());
        Assert.Equal("test-state-parameter", state.GetString());
        
        // Validate response headers per contract
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }

    [Fact]
    public async Task POST_AuthSpotifyLogin_WithInvalidRedirectUri_Returns400BadRequest()
    {
        // Arrange - Invalid redirect URI per contract
        var invalidRequest = new
        {
            redirectUri = "invalid-not-a-url",
            state = "test-state"
        };
        
        var requestJson = JsonSerializer.Serialize(invalidRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/spotify/login", content);

        // Assert - Validate error response per auth-api.md contract
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        // Validate error response schema per contract
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.True(errorResponse.TryGetProperty("message", out var message));
        Assert.True(errorResponse.TryGetProperty("correlationId", out var correlationId));
        
        Assert.Equal("invalid_request", error.GetString());
        Assert.Contains("redirect URI", message.GetString());
        Assert.NotEmpty(correlationId.GetString());
    }

    [Fact]
    public async Task POST_AuthSpotifyLogin_WithMissingRedirectUri_Returns400BadRequest()
    {
        // Arrange - Missing required field
        var invalidRequest = new
        {
            state = "test-state"
            // redirectUri is missing
        };
        
        var requestJson = JsonSerializer.Serialize(invalidRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/spotify/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.Equal("invalid_request", error.GetString());
    }

    [Fact]
    public async Task POST_AuthSpotifyLogin_WithInvalidContentType_Returns415UnsupportedMediaType()
    {
        // Arrange - Wrong content type
        var content = new StringContent("invalid-data", Encoding.UTF8, "text/plain");

        // Act
        var response = await _client.PostAsync("/api/auth/spotify/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Fact]
    public async Task POST_AuthSpotifyLogin_WithEmptyBody_Returns400BadRequest()
    {
        // Arrange - Empty request body
        var content = new StringContent("", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/spotify/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_AuthSpotifyLogin_ValidatesRateLimitHeaders()
    {
        // Arrange
        var loginRequest = new
        {
            redirectUri = "https://test-app.com/callback"
        };
        
        var requestJson = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/spotify/login", content);

        // Assert - Validate rate limit headers per contract
        Assert.True(response.Headers.Contains("X-RateLimit-Remaining"));
        Assert.True(response.Headers.Contains("X-RateLimit-Reset"));
        
        var rateLimitRemaining = response.Headers.GetValues("X-RateLimit-Remaining").First();
        Assert.True(int.TryParse(rateLimitRemaining, out var remaining));
        Assert.True(remaining >= 0 && remaining <= 5); // Contract specifies 5 per minute
    }
}