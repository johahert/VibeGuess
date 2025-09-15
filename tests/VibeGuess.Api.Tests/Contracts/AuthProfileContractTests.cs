using System.Net;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace VibeGuess.Api.Tests.Contracts;

public class AuthProfileContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthProfileContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GET_AuthMe_WithValidBearerToken_Returns200WithUserProfile()
    {
        // Arrange - Add valid Bearer token per auth-api.md contract
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid-jwt-access-token");

        // Act - GET /api/auth/me endpoint
        var response = await _client.GetAsync("/api/auth/me");

        // Assert - Validate contract response per auth-api.md
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        // Validate required response fields per contract
        Assert.True(responseJson.TryGetProperty("user", out var user));
        Assert.True(responseJson.TryGetProperty("settings", out var settings));
        
        // Validate user object structure per contract
        Assert.True(user.TryGetProperty("id", out var userId));
        Assert.True(user.TryGetProperty("displayName", out var displayName));
        Assert.True(user.TryGetProperty("email", out var email));
        Assert.True(user.TryGetProperty("hasSpotifyPremium", out var hasSpotifyPremium));
        Assert.True(user.TryGetProperty("country", out var country));
        Assert.True(user.TryGetProperty("createdAt", out var createdAt));
        Assert.True(user.TryGetProperty("lastLoginAt", out var lastLoginAt));
        
        // Validate user field types per contract
        Assert.NotEmpty(userId.GetString());
        Assert.NotEmpty(displayName.GetString());
        Assert.NotEmpty(email.GetString());
        Assert.True(hasSpotifyPremium.ValueKind == JsonValueKind.True || hasSpotifyPremium.ValueKind == JsonValueKind.False);
        Assert.NotEmpty(country.GetString());
        Assert.NotEmpty(createdAt.GetString());
        Assert.NotEmpty(lastLoginAt.GetString());
        
        // Validate settings object structure per contract
        Assert.True(settings.TryGetProperty("preferredLanguage", out var preferredLanguage));
        Assert.True(settings.TryGetProperty("enableAudioPreview", out var enableAudioPreview));
        Assert.True(settings.TryGetProperty("defaultQuestionCount", out var defaultQuestionCount));
        Assert.True(settings.TryGetProperty("defaultDifficulty", out var defaultDifficulty));
        Assert.True(settings.TryGetProperty("rememberDeviceSelection", out var rememberDeviceSelection));
        
        // Validate settings field types per contract
        Assert.NotEmpty(preferredLanguage.GetString());
        Assert.True(enableAudioPreview.ValueKind == JsonValueKind.True || enableAudioPreview.ValueKind == JsonValueKind.False);
        Assert.True(defaultQuestionCount.GetInt32() > 0);
        Assert.Contains(defaultDifficulty.GetString(), new[] { "Easy", "Medium", "Hard" });
        Assert.True(rememberDeviceSelection.ValueKind == JsonValueKind.True || rememberDeviceSelection.ValueKind == JsonValueKind.False);
    }

    [Fact]
    public async Task GET_AuthMe_WithoutBearerToken_Returns401Unauthorized()
    {
        // Arrange - No authorization header
        
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert - Validate error response per auth-api.md contract
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        // Validate error response schema per contract
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.True(errorResponse.TryGetProperty("message", out var message));
        Assert.True(errorResponse.TryGetProperty("correlationId", out var correlationId));
        
        Assert.Equal("unauthorized", error.GetString());
        Assert.Contains("Invalid or expired token", message.GetString());
        Assert.NotEmpty(correlationId.GetString());
    }

    [Fact]
    public async Task GET_AuthMe_WithInvalidBearerToken_Returns401Unauthorized()
    {
        // Arrange - Add invalid Bearer token
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-jwt-token");

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.Equal("unauthorized", error.GetString());
    }

    [Fact]
    public async Task GET_AuthMe_WithExpiredBearerToken_Returns401Unauthorized()
    {
        // Arrange - Add expired Bearer token
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "expired-jwt-token-from-test");

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.True(errorResponse.TryGetProperty("message", out var message));
        
        Assert.Equal("unauthorized", error.GetString());
        Assert.Contains("Invalid or expired token", message.GetString());
    }

    [Fact]
    public async Task GET_AuthMe_ValidatesRateLimitingHeaders()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid-jwt-for-rate-limit-test");

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert - Validate rate limiting headers per contract (60 per minute per user)
        Assert.True(response.Headers.Contains("X-RateLimit-Remaining") || 
                   response.Headers.Contains("X-RateLimit-Limit"));
        
        // If rate limit headers are present, validate they contain numeric values
        if (response.Headers.Contains("X-RateLimit-Remaining"))
        {
            var remaining = response.Headers.GetValues("X-RateLimit-Remaining").First();
            Assert.True(int.TryParse(remaining, out var remainingCount));
            Assert.True(remainingCount >= 0 && remainingCount <= 60);
        }
        
        if (response.Headers.Contains("X-RateLimit-Limit"))
        {
            var limit = response.Headers.GetValues("X-RateLimit-Limit").First();
            Assert.True(int.TryParse(limit, out var limitCount));
            Assert.Equal(60, limitCount); // 60 per minute per user per contract
        }
    }

    [Fact]
    public async Task GET_AuthMe_ValidatesCorrelationIdHeader()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid-jwt-correlation-test");
        _client.DefaultRequestHeaders.Add("X-Correlation-ID", "profile-test-correlation-789");

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert - Validate correlation ID is returned per contract
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.Equal("profile-test-correlation-789", correlationId);
    }

    [Fact]
    public async Task GET_AuthMe_WithMalformedAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange - Malformed authorization header (missing Bearer prefix)
        _client.DefaultRequestHeaders.Add("Authorization", "malformed-authorization-header");

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.Equal("unauthorized", error.GetString());
    }
}