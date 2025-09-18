using Microsoft.AspNetCore.Mvc;
using VibeGuess.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace VibeGuess.Api.Controllers;

/// <summary>
/// Controller for health check endpoints.
/// </summary>
[Route("api/health")]
[ApiController]
public class HealthController : BaseApiController
{
    private readonly VibeGuessDbContext _dbContext;
    private readonly ILogger<HealthController> _logger;

    public HealthController(VibeGuessDbContext dbContext, ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> GetHealthStatus([FromQuery] bool detailed = false)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            var checks = await GetHealthChecks(detailed);
            var endTime = DateTime.UtcNow;
            var duration = (endTime - startTime).TotalMilliseconds;
            
            // Determine overall status from checks
            var overallStatus = "Healthy";
            if (checks.Any(c => c.Status == "Unhealthy"))
                overallStatus = "Unhealthy";
            else if (checks.Any(c => c.Status == "Degraded"))
                overallStatus = "Degraded";

            // Build health status response
            var healthStatus = new Dictionary<string, object>
            {
                ["status"] = overallStatus,
                ["timestamp"] = DateTime.UtcNow.ToString("o"),
                ["duration"] = $"{duration:F1}ms",
                ["checks"] = checks.Select(c => new Dictionary<string, object>
                {
                    ["name"] = c.Name,
                    ["status"] = c.Status,
                    ["duration"] = c.Duration,
                    ["description"] = c.Description,
                    ["data"] = c.Data
                }).ToArray()
            };

            // Add detailed info if requested
            if (detailed)
            {
                healthStatus["version"] = "1.0.0";
                healthStatus["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                healthStatus["uptime"] = $"{DateTime.UtcNow.Subtract(Process.GetCurrentProcess().StartTime):g}";
            }

            // Add cache headers for brief caching (30 seconds)
            Response.Headers.CacheControl = "public, max-age=30";

            var statusCode = overallStatus == "Unhealthy" ? 503 : 200;
            
            // Add rate limiting and correlation headers
            AddRateLimitHeaders();
            
            return StatusCode(statusCode, healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            var endTime = DateTime.UtcNow;
            var duration = (endTime - startTime).TotalMilliseconds;
            
            var unhealthyStatus = new
            {
                status = "Unhealthy",
                timestamp = DateTime.UtcNow.ToString("o"),
                duration = $"{duration:F1}ms",
                checks = new[]
                {
                    new { name = "api", status = "Unhealthy", duration = $"{duration:F1}ms" }
                }
            };

            // Add headers for error case too
            AddRateLimitHeaders();

            return StatusCode(503, unhealthyStatus);
        }
    }

    private async Task<List<HealthCheckResult>> GetHealthChecks(bool detailed)
    {
        var checks = new List<HealthCheckResult>();
        
        // Always include API check
        var apiStartTime = DateTime.UtcNow;
        checks.Add(new HealthCheckResult
        {
            Name = "api",
            Status = "Healthy",
            Duration = $"{(DateTime.UtcNow - apiStartTime).TotalMilliseconds:F1}ms",
            Description = "API endpoint health",
            Data = new { endpoint = "/api/health", method = "GET" }
        });

        // Database check (always included)
        var dbStartTime = DateTime.UtcNow;
        try
        {
            await _dbContext.Database.CanConnectAsync();
            checks.Add(new HealthCheckResult
            {
                Name = "database",
                Status = "Healthy",
                Duration = $"{(DateTime.UtcNow - dbStartTime).TotalMilliseconds:F1}ms",
                Description = "Database connectivity check",
                Data = new { provider = "EntityFramework", connected = true }
            });
        }
        catch (Exception ex)
        {
            checks.Add(new HealthCheckResult
            {
                Name = "database",
                Status = "Unhealthy",
                Duration = $"{(DateTime.UtcNow - dbStartTime).TotalMilliseconds:F1}ms",
                Description = "Database connectivity check",
                Data = new { provider = "EntityFramework", connected = false, error = ex.Message }
            });
            _logger.LogWarning(ex, "Database health check failed");
        }

        if (detailed)
        {
            // Memory check
            var memStartTime = DateTime.UtcNow;
            var workingSet = GC.GetTotalMemory(false);
            checks.Add(new HealthCheckResult
            {
                Name = "memory",
                Status = workingSet < 500 * 1024 * 1024 ? "Healthy" : "Degraded", // 500MB threshold
                Duration = $"{(DateTime.UtcNow - memStartTime).TotalMilliseconds:F1}ms",
                Description = "Memory usage check",
                Data = new { usageBytes = workingSet, thresholdBytes = 500 * 1024 * 1024 }
            });
        }

        return checks;
    }

    private class HealthCheckResult
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}