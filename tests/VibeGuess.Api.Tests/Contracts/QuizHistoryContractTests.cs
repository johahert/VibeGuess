using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit;

namespace VibeGuess.Api.Tests.Contracts;

/// <summary>
/// Contract tests for GET /api/quiz/my-quizzes endpoint.
/// These tests MUST FAIL initially (TDD RED phase) before implementation.
/// </summary>
public class QuizHistoryContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public QuizHistoryContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetMyQuizzes_WithValidAuthentication_ShouldReturn200WithQuizList()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/quiz/my-quizzes");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var quizzes = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.Equal(JsonValueKind.Array, quizzes.ValueKind);
        
        // If there are quizzes, validate structure
        if (quizzes.GetArrayLength() > 0)
        {
            var firstQuiz = quizzes[0];
            Assert.True(firstQuiz.TryGetProperty("id", out _));
            Assert.True(firstQuiz.TryGetProperty("title", out _));
            Assert.True(firstQuiz.TryGetProperty("createdAt", out _));
            Assert.True(firstQuiz.TryGetProperty("questionsCount", out _));
            Assert.True(firstQuiz.TryGetProperty("difficulty", out _));
        }
    }

    [Fact]
    public async Task GetMyQuizzes_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No authentication header

        // Act
        var response = await _client.GetAsync("/api/quiz/my-quizzes");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("unauthorized", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task GetMyQuizzes_WithExpiredToken_ShouldReturn401Unauthorized()
    {
        // Arrange
        var expiredToken = "Bearer expired.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "expired.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/quiz/my-quizzes");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("token", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task GetMyQuizzes_WithPaginationParameters_ShouldReturnPaginatedResults()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/quiz/my-quizzes?page=1&limit=10");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(result.TryGetProperty("quizzes", out var quizzesProperty));
        Assert.True(result.TryGetProperty("totalCount", out _));
        Assert.True(result.TryGetProperty("page", out _));
        Assert.True(result.TryGetProperty("pageSize", out _));
        Assert.True(result.TryGetProperty("hasNextPage", out _));
        Assert.True(result.TryGetProperty("hasPreviousPage", out _));
        
        Assert.Equal(JsonValueKind.Array, quizzesProperty.ValueKind);
    }

    [Fact]
    public async Task GetMyQuizzes_WithInvalidPaginationParameters_ShouldReturn400BadRequest()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/quiz/my-quizzes?page=-1&limit=0");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(error.TryGetProperty("error", out var errorProperty));
        Assert.Contains("invalid", errorProperty.GetString().ToLower());
    }

    [Fact]
    public async Task GetMyQuizzes_WithSortingParameters_ShouldReturnSortedResults()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/quiz/my-quizzes?sortBy=createdAt&sortOrder=desc");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var quizzes = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.Equal(JsonValueKind.Array, quizzes.ValueKind);
        
        // If multiple quizzes exist, verify sorting
        if (quizzes.GetArrayLength() > 1)
        {
            var firstDate = DateTime.Parse(quizzes[0].GetProperty("createdAt").GetString());
            var secondDate = DateTime.Parse(quizzes[1].GetProperty("createdAt").GetString());
            Assert.True(firstDate >= secondDate, "Quizzes should be sorted by createdAt descending");
        }
    }

    [Fact]
    public async Task GetMyQuizzes_WithFilterParameters_ShouldReturnFilteredResults()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/quiz/my-quizzes?difficulty=medium&status=completed");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var quizzes = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.Equal(JsonValueKind.Array, quizzes.ValueKind);
        
        // If quizzes exist, verify filtering
        foreach (var quiz in quizzes.EnumerateArray())
        {
            Assert.Equal("medium", quiz.GetProperty("difficulty").GetString().ToLower());
            Assert.Equal("completed", quiz.GetProperty("status").GetString().ToLower());
        }
    }

    [Fact]
    public async Task GetMyQuizzes_ShouldReturnCorrectContentType()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/quiz/my-quizzes");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetMyQuizzes_ResponseTime_ShouldBeFast()
    {
        // Arrange
        var validToken = "Bearer valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid.jwt.token");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/quiz/my-quizzes");
        stopwatch.Stop();

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        // Response should be under 3 seconds (may need to query database)
        Assert.True(stopwatch.ElapsedMilliseconds < 3000, 
            $"Response took {stopwatch.ElapsedMilliseconds}ms, expected < 3000ms");
    }
}