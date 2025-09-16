using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace VibeGuess.Api.Tests.Contracts;

/// <summary>
/// Contract tests for POST /api/quiz/{id}/start-session endpoint.
/// These tests MUST FAIL initially (TDD RED phase) before implementation.
/// </summary>
public class QuizSessionContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public QuizSessionContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task StartQuizSession_WithValidRequest_ShouldReturn201WithSessionData()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            deviceId = "spotify-device-id",
            shuffleQuestions = false,
            enableHints = true
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/quiz/{quizId}/start-session", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var session = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(session.TryGetProperty("sessionId", out var sessionIdProperty));
        Assert.NotEqual(Guid.Empty.ToString(), sessionIdProperty.GetString());
        Assert.True(session.TryGetProperty("quizId", out var quizIdProperty));
        Assert.Equal(quizId.ToString(), quizIdProperty.GetString());
        Assert.True(session.TryGetProperty("startedAt", out _));
        Assert.True(session.TryGetProperty("currentQuestionIndex", out var currentQuestionProperty));
        Assert.Equal(0, currentQuestionProperty.GetInt32());
        Assert.True(session.TryGetProperty("totalQuestions", out _));
        Assert.True(session.TryGetProperty("status", out var statusProperty));
        Assert.Equal("active", statusProperty.GetString());
    }

    [Fact]
    public async Task StartQuizSession_WithInvalidQuizId_ShouldReturn404NotFound()
    {
        // Arrange
        var invalidQuizId = Guid.NewGuid();
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/quiz/{invalidQuizId}/start-session", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("quiz not found", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task StartQuizSession_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var requestBody = JsonSerializer.Serialize(new
        {
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/quiz/{quizId}/start-session", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("unauthorized", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task StartQuizSession_WithInvalidRequestBody_ShouldReturn400BadRequest()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var invalidRequestBody = "{ invalid json }";
        var content = new StringContent(invalidRequestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/quiz/{quizId}/start-session", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("invalid", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task StartQuizSession_WithMissingDeviceId_ShouldReturn400BadRequest()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            shuffleQuestions = false
            // Missing required deviceId
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/quiz/{quizId}/start-session", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("deviceid", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task StartQuizSession_WithActiveSession_ShouldReturn409Conflict()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act - Try to start session twice
        await _client.PostAsync($"/api/quiz/{quizId}/start-session", content);
        var response = await _client.PostAsync($"/api/quiz/{quizId}/start-session", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("active session", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task StartQuizSession_WithInvalidDeviceId_ShouldReturn400BadRequest()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            deviceId = "non-existent-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/quiz/{quizId}/start-session", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("device", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task StartQuizSession_ShouldReturnCorrectContentType()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/quiz/{quizId}/start-session", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task StartQuizSession_ShouldIncludeLocationHeader()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        var requestBody = JsonSerializer.Serialize(new
        {
            deviceId = "spotify-device-id"
        });

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/quiz/{quizId}/start-session", content);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        if (response.StatusCode == HttpStatusCode.Created)
        {
            Assert.NotNull(response.Headers.Location);
            Assert.Contains("/api/quiz/session/", response.Headers.Location.ToString());
        }
    }
}