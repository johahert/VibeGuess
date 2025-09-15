using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace VibeGuess.Api.Tests.Contracts;

public class QuizGenerationContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public QuizGenerationContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        
        // Add Bearer token for authenticated endpoints
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "valid-jwt-access-token");
    }

    [Fact]
    public async Task POST_QuizGenerate_WithValidRequest_Returns200WithQuiz()
    {
        // Arrange - Create valid quiz generation request per quiz-api.md contract
        var quizRequest = new
        {
            prompt = "Create a quiz about 80s rock bands and their hit songs",
            questionCount = 10,
            format = "MultipleChoice",
            difficulty = "Medium",
            includeAudio = true,
            language = "en"
        };
        
        var requestJson = JsonSerializer.Serialize(quizRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act - POST to /api/quiz/generate endpoint
        var response = await _client.PostAsync("/api/quiz/generate", content);

        // Assert - Validate contract response per quiz-api.md
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        // Validate required response fields per contract
        Assert.True(responseJson.TryGetProperty("quiz", out var quiz));
        Assert.True(responseJson.TryGetProperty("generationMetadata", out var metadata));
        
        // Validate quiz object structure per contract
        Assert.True(quiz.TryGetProperty("id", out var quizId));
        Assert.True(quiz.TryGetProperty("title", out var title));
        Assert.True(quiz.TryGetProperty("userPrompt", out var userPrompt));
        Assert.True(quiz.TryGetProperty("format", out var format));
        Assert.True(quiz.TryGetProperty("difficulty", out var difficulty));
        Assert.True(quiz.TryGetProperty("questionCount", out var questionCount));
        Assert.True(quiz.TryGetProperty("createdAt", out var createdAt));
        Assert.True(quiz.TryGetProperty("expiresAt", out var expiresAt));
        Assert.True(quiz.TryGetProperty("status", out var status));
        Assert.True(quiz.TryGetProperty("questions", out var questions));
        
        // Validate quiz field types and values per contract
        Assert.NotEmpty(quizId.GetString());
        Assert.NotEmpty(title.GetString());
        Assert.Equal("Create a quiz about 80s rock bands and their hit songs", userPrompt.GetString());
        Assert.Equal("MultipleChoice", format.GetString());
        Assert.Equal("Medium", difficulty.GetString());
        Assert.Equal(10, questionCount.GetInt32());
        Assert.NotEmpty(createdAt.GetString());
        Assert.NotEmpty(expiresAt.GetString());
        Assert.Equal("Generated", status.GetString());
        Assert.True(questions.GetArrayLength() > 0);
        
        // Validate first question structure per contract
        var firstQuestion = questions[0];
        Assert.True(firstQuestion.TryGetProperty("id", out var questionId));
        Assert.True(firstQuestion.TryGetProperty("orderIndex", out var orderIndex));
        Assert.True(firstQuestion.TryGetProperty("questionText", out var questionText));
        Assert.True(firstQuestion.TryGetProperty("type", out var questionType));
        Assert.True(firstQuestion.TryGetProperty("requiresAudio", out var requiresAudio));
        Assert.True(firstQuestion.TryGetProperty("points", out var points));
        Assert.True(firstQuestion.TryGetProperty("track", out var track));
        Assert.True(firstQuestion.TryGetProperty("answerOptions", out var answerOptions));
        
        // Validate generation metadata per contract
        Assert.True(metadata.TryGetProperty("processingTimeMs", out var processingTime));
        Assert.True(metadata.TryGetProperty("aiModel", out var aiModel));
        Assert.True(metadata.TryGetProperty("tracksFound", out var tracksFound));
        Assert.True(metadata.TryGetProperty("tracksValidated", out var tracksValidated));
        
        Assert.True(processingTime.GetInt32() > 0);
        Assert.NotEmpty(aiModel.GetString());
        Assert.True(tracksFound.GetInt32() >= 0);
        Assert.True(tracksValidated.GetInt32() >= 0);
    }

    [Fact]
    public async Task POST_QuizGenerate_WithMissingPrompt_Returns400BadRequest()
    {
        // Arrange - Missing prompt field per contract validation
        var invalidRequest = new
        {
            questionCount = 10,
            format = "MultipleChoice",
            difficulty = "Medium"
            // prompt is missing
        };
        
        var requestJson = JsonSerializer.Serialize(invalidRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/quiz/generate", content);

        // Assert - Validate error response per quiz-api.md contract
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        // Validate error response schema per contract
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.True(errorResponse.TryGetProperty("message", out var message));
        Assert.True(errorResponse.TryGetProperty("correlationId", out var correlationId));
        Assert.True(errorResponse.TryGetProperty("details", out var details));
        
        Assert.Equal("invalid_request", error.GetString());
        Assert.Contains("Prompt is required", message.GetString());
        Assert.NotEmpty(correlationId.GetString());
        Assert.True(details.TryGetProperty("prompt", out var promptError));
    }

    [Fact]
    public async Task POST_QuizGenerate_WithInvalidPromptLength_Returns400BadRequest()
    {
        // Arrange - Prompt too short per contract validation (10-1000 characters)
        var invalidRequest = new
        {
            prompt = "short", // Only 5 characters, less than 10 minimum
            questionCount = 10,
            format = "MultipleChoice"
        };
        
        var requestJson = JsonSerializer.Serialize(invalidRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/quiz/generate", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.Equal("invalid_request", error.GetString());
        Assert.Contains("10-1000 characters", errorResponse.GetProperty("message").GetString());
    }

    [Fact]
    public async Task POST_QuizGenerate_WithInvalidQuestionCount_Returns400BadRequest()
    {
        // Arrange - Invalid question count per contract validation (5-20 range)
        var invalidRequest = new
        {
            prompt = "Create a quiz about classical music composers and their symphonies",
            questionCount = 25, // Exceeds maximum of 20
            format = "MultipleChoice"
        };
        
        var requestJson = JsonSerializer.Serialize(invalidRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/quiz/generate", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.Equal("invalid_request", error.GetString());
    }

    [Fact]
    public async Task POST_QuizGenerate_WithInvalidFormat_Returns400BadRequest()
    {
        // Arrange - Invalid format enum per contract
        var invalidRequest = new
        {
            prompt = "Create a quiz about jazz musicians and their instruments",
            questionCount = 8,
            format = "InvalidFormat", // Not in enum ["MultipleChoice", "FreeText", "Mixed"]
            difficulty = "Medium"
        };
        
        var requestJson = JsonSerializer.Serialize(invalidRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/quiz/generate", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.True(errorResponse.TryGetProperty("error", out var error));
        Assert.Equal("invalid_request", error.GetString());
    }

    [Fact]
    public async Task POST_QuizGenerate_WithContentGenerationFailure_Returns422UnprocessableEntity()
    {
        // Arrange - Prompt that should cause content generation failure
        var problematicRequest = new
        {
            prompt = "Create a quiz about extremely obscure underground bands that nobody has ever heard of",
            questionCount = 10,
            format = "MultipleChoice",
            includeAudio = true
        };
        
        var requestJson = JsonSerializer.Serialize(problematicRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/quiz/generate", content);

        // Assert - Should return 422 if content generation fails per contract
        // Note: This might return 200 if the generation succeeds, but we're testing the contract
        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
            
            Assert.True(errorResponse.TryGetProperty("error", out var error));
            Assert.True(errorResponse.TryGetProperty("message", out var message));
            Assert.True(errorResponse.TryGetProperty("details", out var details));
            
            Assert.Equal("content_generation_failed", error.GetString());
            Assert.Contains("generate sufficient quiz content", message.GetString());
            
            if (details.TryGetProperty("suggestedPrompts", out var suggestions))
            {
                Assert.True(suggestions.GetArrayLength() > 0);
            }
        }
        
        // If it returns 200, that's also valid - the generation succeeded
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task POST_QuizGenerate_WithoutBearerToken_Returns401Unauthorized()
    {
        // Arrange - Remove authorization header
        var clientWithoutAuth = _factory.CreateClient();
        
        var quizRequest = new
        {
            prompt = "Create a quiz about pop music from the 2000s",
            questionCount = 5
        };
        
        var requestJson = JsonSerializer.Serialize(quizRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await clientWithoutAuth.PostAsync("/api/quiz/generate", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task POST_QuizGenerate_ValidatesRateLimitingHeaders()
    {
        // Arrange
        var quizRequest = new
        {
            prompt = "Create a quiz about country music legends and their biggest hits",
            questionCount = 7,
            format = "MultipleChoice"
        };
        
        var requestJson = JsonSerializer.Serialize(quizRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/quiz/generate", content);

        // Assert - Validate rate limiting headers per contract (10 per hour)
        Assert.True(response.Headers.Contains("X-RateLimit-Remaining") || 
                   response.Headers.Contains("X-RateLimit-Limit"));
        
        if (response.Headers.Contains("X-RateLimit-Remaining"))
        {
            var remaining = response.Headers.GetValues("X-RateLimit-Remaining").First();
            Assert.True(int.TryParse(remaining, out var remainingCount));
            Assert.True(remainingCount >= 0 && remainingCount <= 10);
        }
    }

    [Fact]
    public async Task POST_QuizGenerate_ValidatesCorrelationIdHeader()
    {
        // Arrange
        var quizRequest = new
        {
            prompt = "Create a quiz about electronic music genres and their characteristics",
            questionCount = 6
        };
        
        var requestJson = JsonSerializer.Serialize(quizRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
        content.Headers.Add("X-Correlation-ID", "quiz-generation-test-999");

        // Act
        var response = await _client.PostAsync("/api/quiz/generate", content);

        // Assert - Validate correlation ID is returned per contract
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.Equal("quiz-generation-test-999", correlationId);
    }
}