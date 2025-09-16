using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit;

namespace VibeGuess.Api.Tests.Contracts;

/// <summary>
/// Contract tests for GET /api/health endpoint.
/// These tests MUST FAIL initially (TDD RED phase) before implementation.
/// </summary>
public class HealthCheckContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HealthCheckContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetHealthCheck_ShouldReturn200WithHealthStatus()
    {
        // Arrange - No authentication required for basic health check

        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(healthStatus.TryGetProperty("status", out var statusProperty));
        Assert.True(statusProperty.GetString() == "Healthy" || 
                   statusProperty.GetString() == "Degraded" ||
                   statusProperty.GetString() == "Unhealthy");
        
        Assert.True(healthStatus.TryGetProperty("timestamp", out _));
        Assert.True(healthStatus.TryGetProperty("duration", out _));
        Assert.True(healthStatus.TryGetProperty("checks", out var checksProperty));
        Assert.Equal(JsonValueKind.Array, checksProperty.ValueKind);
        
        // Basic API health should always be included
        bool hasApiCheck = false;
        foreach (var check in checksProperty.EnumerateArray())
        {
            Assert.True(check.TryGetProperty("name", out _));
            Assert.True(check.TryGetProperty("status", out _));
            Assert.True(check.TryGetProperty("duration", out _));
            
            if (check.TryGetProperty("name", out var nameProperty) && 
                nameProperty.GetString() == "api")
            {
                hasApiCheck = true;
            }
        }
        Assert.True(hasApiCheck, "API health check should be included");
    }

    [Fact]
    public async Task GetHealthCheck_ShouldIncludeDatabaseCheck()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(healthStatus.TryGetProperty("checks", out var checksProperty));
        
        // Database health check should be included
        bool hasDatabaseCheck = false;
        foreach (var check in checksProperty.EnumerateArray())
        {
            if (check.TryGetProperty("name", out var nameProperty) && 
                nameProperty.GetString() == "database")
            {
                hasDatabaseCheck = true;
                Assert.True(check.TryGetProperty("status", out var dbStatusProperty));
                Assert.True(dbStatusProperty.GetString() == "Healthy" || 
                           dbStatusProperty.GetString() == "Degraded" ||
                           dbStatusProperty.GetString() == "Unhealthy");
                break;
            }
        }
        Assert.True(hasDatabaseCheck, "Database health check should be included");
    }

    [Fact]
    public async Task GetHealthCheck_WithDetailedQuery_ShouldIncludeExtendedInfo()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("/api/health?detailed=true");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(healthStatus.TryGetProperty("version", out _));
        Assert.True(healthStatus.TryGetProperty("environment", out _));
        Assert.True(healthStatus.TryGetProperty("uptime", out _));
        
        // Detailed checks should include more information
        Assert.True(healthStatus.TryGetProperty("checks", out var checksProperty));
        foreach (var check in checksProperty.EnumerateArray())
        {
            Assert.True(check.TryGetProperty("description", out _));
            Assert.True(check.TryGetProperty("data", out _));
        }
    }

    [Fact]
    public async Task GetHealthCheck_ShouldReturn503WhenUnhealthy()
    {
        // Arrange - Force unhealthy state (this might require special test configuration)

        // Act
        var response = await _client.GetAsync("/api/health?forceUnhealthy=true");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.True(response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                   response.StatusCode == HttpStatusCode.OK); // May still return 200 with Unhealthy status
        
        var content = await response.Content.ReadAsStringAsync();
        var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(healthStatus.TryGetProperty("status", out var statusProperty));
        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            Assert.Equal("Unhealthy", statusProperty.GetString());
        }
    }

    [Fact]
    public async Task GetHealthCheck_ShouldReturnCorrectContentType()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetHealthCheck_ShouldBeFast()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/health");
        stopwatch.Stop();

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        // Health check should be very fast (under 5 seconds)
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Health check took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
    }

    [Fact]
    public async Task GetHealthCheck_ShouldSupportCaching()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("If-None-Match", "\"health-etag\"");

        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        // Health check may support caching for brief periods
        Assert.True(response.StatusCode == HttpStatusCode.NotModified || 
                   response.StatusCode == HttpStatusCode.OK);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            // Should include cache headers for brief caching (e.g., 30 seconds)
            Assert.True(response.Headers.CacheControl?.MaxAge <= TimeSpan.FromMinutes(1));
        }
    }

    [Fact]
    public async Task GetHealthCheck_ShouldIncludeCorrelationId()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        _client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);

        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        if (response.Headers.Contains("X-Correlation-ID"))
        {
            var responseCorrelationId = response.Headers.GetValues("X-Correlation-ID").First();
            Assert.Equal(correlationId, responseCorrelationId);
        }
    }

    [Fact]
    public async Task GetHealthCheck_WithMultipleConcurrentRequests_ShouldHandleLoad()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
        
        // Act - Send 10 concurrent health check requests
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetAsync("/api/health"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - This MUST FAIL initially (404 Not Found expected until implementation)
        foreach (var response in responses)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.True(healthStatus.TryGetProperty("status", out _));
        }

        // Cleanup
        foreach (var response in responses)
        {
            response.Dispose();
        }
    }
}