using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace VibeGuess.Api.Tests.Contracts;

public class AuthRefreshContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthRefreshContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task POST_AuthRefresh_WithValidRefreshToken_Returns200WithNewTokens()
    {
        // Arrange - Create valid refresh request per auth-api.md contract
        var refreshRequest = new
        {
            refreshToken = "valid-refresh-token-from-previous-authentication"
        };
        
        var requestJson = JsonSerializer.Serialize(refreshRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act - POST to /api/auth/refresh endpoint
        var response = await _client.PostAsync("/api/auth/refresh", content);

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
        
        // Validate response field types and values per contract
        Assert.NotEmpty(accessToken.GetString());
        Assert.NotEmpty(refreshToken.GetString());
        Assert.Equal(3600, expiresIn.GetInt32());
        Assert.Equal("Bearer", tokenType.GetString());
        
        // Validate that new tokens are different from input (token rotation)
        Assert.NotEqual("valid-refresh-token-from-previous-authentication", refreshToken.GetString());
    }

    [Fact]
    public async Task POST_AuthRefresh_WithInvalidRefreshToken_Returns401Unauthorized()
    {
        // Arrange - Invalid refresh token per contract
        var invalidRequest = new
        {
            refreshToken = "invalid-or-expired-refresh-token"
        };
        
        var requestJson = JsonSerializer.Serialize(invalidRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/refresh", content);

        // Assert - Validate error response per auth-api.md contract
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        // Validate error response schema per contract
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.True(errorResponse.TryGetProperty("message", out var message));
        Assert.True(errorResponse.TryGetProperty("correlationId", out var correlationId));
        
        Assert.Equal("invalid_grant", error.GetString());
        Assert.Contains("Invalid or expired refresh token", message.GetString());
        Assert.NotEmpty(correlationId.GetString());
    }

    [Fact]
    public async Task POST_AuthRefresh_WithMissingRefreshToken_Returns400BadRequest()
    {
        // Arrange - Missing refresh token field
        var invalidRequest = new
        {
            // refreshToken is missing
        };
        
        var requestJson = JsonSerializer.Serialize(invalidRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/refresh", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.True(errorResponse.TryGetProperty("message", out var message));
        Assert.True(errorResponse.TryGetProperty("correlationId", out var correlationId));
        
        Assert.Equal("invalid_request", error.GetString());
        Assert.Contains("refreshToken", message.GetString());
        Assert.NotEmpty(correlationId.GetString());
    }

    [Fact]
    public async Task POST_AuthRefresh_WithEmptyRefreshToken_Returns400BadRequest()
    {
        // Arrange - Empty refresh token
        var invalidRequest = new
        {
            refreshToken = ""
        };
        
        var requestJson = JsonSerializer.Serialize(invalidRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/refresh", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.Equal("invalid_request", error.GetString());
    }

    [Fact]
    public async Task POST_AuthRefresh_ValidatesRateLimitingHeaders()
    {
        // Arrange
        var refreshRequest = new
        {
            refreshToken = "valid-refresh-token-for-rate-limit-test"
        };
        
        var requestJson = JsonSerializer.Serialize(refreshRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/refresh", content);

        // Assert - Validate rate limiting headers per contract
        Assert.True(response.Headers.Contains("X-RateLimit-Remaining") || 
                   response.Headers.Contains("X-RateLimit-Limit"));
        
        // If rate limit headers are present, validate they contain numeric values
        if (response.Headers.Contains("X-RateLimit-Remaining"))
        {
            var remaining = response.Headers.GetValues("X-RateLimit-Remaining").First();
            Assert.True(int.TryParse(remaining, out var remainingCount));
            Assert.True(remainingCount >= 0);
        }
    }

    [Fact]
    public async Task POST_AuthRefresh_ValidatesCorrelationIdHeader()
    {
        // Arrange
        var refreshRequest = new
        {
            refreshToken = "valid-refresh-token-correlation-test"
        };
        
        var requestJson = JsonSerializer.Serialize(refreshRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
        content.Headers.Add("X-Correlation-ID", "refresh-test-correlation-456");

        // Act
        var response = await _client.PostAsync("/api/auth/refresh", content);

        // Assert - Validate correlation ID is returned per contract
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.Equal("refresh-test-correlation-456", correlationId);
    }

    [Fact]
    public async Task POST_AuthRefresh_WithMalformedJson_Returns400BadRequest()
    {
        // Arrange - Malformed JSON
        var malformedJson = "{ refreshToken: invalid-json-format }";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/refresh", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.Equal("invalid_request", error.GetString());
    }
}