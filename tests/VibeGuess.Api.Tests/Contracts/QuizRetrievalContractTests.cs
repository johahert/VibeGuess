using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit;

namespace VibeGuess.Api.Tests.Contracts;

/// <summary>
/// Contract tests for GET /api/quiz/{id} endpoint.
/// These tests MUST FAIL initially (TDD RED phase) before implementation.
/// </summary>
public class QuizRetrievalContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public QuizRetrievalContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetQuiz_WithValidId_ShouldReturn200WithQuizData()
    {
        // Arrange
        var quizId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/quiz/{quizId}");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var quiz = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(quiz.TryGetProperty("id", out var idProperty));
        Assert.Equal(quizId.ToString(), idProperty.GetString());
        Assert.True(quiz.TryGetProperty("title", out _));
        Assert.True(quiz.TryGetProperty("description", out _));
        Assert.True(quiz.TryGetProperty("questions", out _));
        Assert.True(quiz.TryGetProperty("difficulty", out _));
        Assert.True(quiz.TryGetProperty("createdAt", out _));
        Assert.True(quiz.TryGetProperty("estimatedDuration", out _));
    }

    [Fact]
    public async Task GetQuiz_WithInvalidGuid_ShouldReturn400BadRequest()
    {
        // Arrange
        var invalidId = "invalid-guid";

        // Act
        var response = await _client.GetAsync($"/api/quiz/{invalidId}");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("invalid", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task GetQuiz_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/quiz/{nonExistentId}");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("not found", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task GetQuiz_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange
        var quizId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/quiz/{quizId}");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("unauthorized", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task GetQuiz_WithExpiredToken_ShouldReturn401Unauthorized()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var expiredToken = "Bearer expired.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "expired.jwt.token");

        // Act
        var response = await _client.GetAsync($"/api/quiz/{quizId}");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("token", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task GetQuiz_WithValidAuthentication_ShouldIncludeUserSpecificData()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        // Act
        var response = await _client.GetAsync($"/api/quiz/{quizId}");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var quiz = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(quiz.TryGetProperty("userProgress", out _));
        Assert.True(quiz.TryGetProperty("canEdit", out _));
        Assert.True(quiz.TryGetProperty("isBookmarked", out _));
    }

    [Fact]
    public async Task GetQuiz_WithValidId_ShouldReturnCorrectContentType()
    {
        // Arrange
        var quizId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/quiz/{quizId}");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetQuiz_WithCacheHeaders_ShouldRespectCaching()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Add("If-None-Match", "\"quiz-etag\"");

        // Act
        var response = await _client.GetAsync($"/api/quiz/{quizId}");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        // Should return 304 Not Modified if quiz hasn't changed
        Assert.True(response.StatusCode == HttpStatusCode.NotModified || response.StatusCode == HttpStatusCode.OK);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            Assert.True(response.Headers.ETag != null);
        }
    }

    [Fact]
    public async Task GetQuiz_ResponseTime_ShouldBeFast()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync($"/api/quiz/{quizId}");
        stopwatch.Stop();

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        // Response should be under 2 seconds
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, 
            $"Response took {stopwatch.ElapsedMilliseconds}ms, expected < 2000ms");
    }
}