using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace VibeGuess.Api.Tests.Contracts;

public class AuthCallbackContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthCallbackContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task POST_AuthSpotifyCallback_WithValidRequest_Returns200WithTokens()
    {
        // Arrange - Create valid callback request per auth-api.md contract
        var callbackRequest = new
        {
            code = "valid-authorization-code-from-spotify",
            codeVerifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk",
            redirectUri = "https://test-app.com/callback"
        };
        
        var requestJson = JsonSerializer.Serialize(callbackRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act - POST to /api/auth/spotify/callback endpoint
        var response = await _client.PostAsync("/api/auth/spotify/callback", content);

        // Assert - Validate contract response per auth-api.md
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        // Validate required response fields per contract
        Assert.True(responseJson.TryGetProperty("accessToken", out var accessToken));
        Assert.True(responseJson.TryGetProperty("refreshToken", out var refreshToken));
        Assert.True(responseJson.TryGetProperty("expiresIn", out var expiresIn));
        Assert.True(responseJson.TryGetProperty("tokenType", out var tokenType));
        Assert.True(responseJson.TryGetProperty("user", out var user));
        
        // Validate response field types per contract
        Assert.NotEmpty(accessToken.GetString());
        Assert.NotEmpty(refreshToken.GetString());
        Assert.Equal(3600, expiresIn.GetInt32());
        Assert.Equal("Bearer", tokenType.GetString());
        
        // Validate user object structure per contract
        Assert.True(user.TryGetProperty("id", out var userId));
        Assert.True(user.TryGetProperty("displayName", out var displayName));
        Assert.True(user.TryGetProperty("email", out var email));
        Assert.True(user.TryGetProperty("hasSpotifyPremium", out var hasSpotifyPremium));
        Assert.True(user.TryGetProperty("country", out var country));
        
        Assert.NotEmpty(userId.GetString());
        Assert.NotEmpty(displayName.GetString());
        Assert.NotEmpty(email.GetString());
        Assert.True(hasSpotifyPremium.ValueKind == JsonValueKind.True || hasSpotifyPremium.ValueKind == JsonValueKind.False);
        Assert.NotEmpty(country.GetString());
    }

    [Fact]
    public async Task POST_AuthSpotifyCallback_WithInvalidCode_Returns400BadRequest()
    {
        // Arrange - Invalid authorization code per contract
        var invalidRequest = new
        {
            code = "invalid-authorization-code",
            codeVerifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk",
            redirectUri = "https://test-app.com/callback"
        };
        
        var requestJson = JsonSerializer.Serialize(invalidRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/spotify/callback", content);

        // Assert - Validate error response per auth-api.md contract
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        // Validate error response schema per contract
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.True(errorResponse.TryGetProperty("message", out var message));
        Assert.True(errorResponse.TryGetProperty("correlationId", out var correlationId));
        
        Assert.Equal("invalid_grant", error.GetString());
        Assert.Contains("authorization code", message.GetString());
        Assert.NotEmpty(correlationId.GetString());
    }

    [Fact]
    public async Task POST_AuthSpotifyCallback_WithInvalidCodeVerifier_Returns400BadRequest()
    {
        // Arrange - Invalid code verifier per contract
        var invalidRequest = new
        {
            code = "valid-authorization-code-from-spotify",
            codeVerifier = "invalid-code-verifier",
            redirectUri = "https://test-app.com/callback"
        };
        
        var requestJson = JsonSerializer.Serialize(invalidRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/spotify/callback", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.Equal("invalid_grant", error.GetString());
    }

    [Fact]
    public async Task POST_AuthSpotifyCallback_WithMissingRequiredFields_Returns400BadRequest()
    {
        // Arrange - Missing code field
        var invalidRequest = new
        {
            codeVerifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk",
            redirectUri = "https://test-app.com/callback"
            // code is missing
        };
        
        var requestJson = JsonSerializer.Serialize(invalidRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/spotify/callback", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.Equal("invalid_request", error.GetString());
    }

    [Fact]
    public async Task POST_AuthSpotifyCallback_ValidatesCorrelationIdHeader()
    {
        // Arrange
        var callbackRequest = new
        {
            code = "valid-authorization-code-from-spotify",
            codeVerifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk",
            redirectUri = "https://test-app.com/callback"
        };
        
        var requestJson = JsonSerializer.Serialize(callbackRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
        content.Headers.Add("X-Correlation-ID", "test-correlation-123");

        // Act
        var response = await _client.PostAsync("/api/auth/spotify/callback", content);

        // Assert - Validate correlation ID is returned per contract
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.Equal("test-correlation-123", correlationId);
    }
}