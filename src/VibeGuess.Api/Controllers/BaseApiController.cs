using Microsoft.AspNetCore.Mvc;
using VibeGuess.Api.Models;

namespace VibeGuess.Api.Controllers;

/// <summary>
/// Base controller with common functionality like rate limiting and error handling.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    private const int RateLimitMax = 5;
    private const int RateLimitWindowMinutes = 1;

    /// <summary>
    /// Creates a standardized error response.
    /// </summary>
    protected ObjectResult CreateErrorResponse(int statusCode, string errorCode, string message)
    {
        var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
        
        var errorResponse = new ErrorResponse
        {
            Error = errorCode,
            Message = message,
            CorrelationId = correlationId
        };

        return StatusCode(statusCode, errorResponse);
    }

    /// <summary>
    /// Creates a 400 Bad Request error response.
    /// </summary>
    protected ObjectResult BadRequestWithError(string message)
    {
        AddRateLimitHeaders();
        return CreateErrorResponse(400, "invalid_request", message);
    }

    /// <summary>
    /// Creates an Ok response with rate limiting headers.
    /// </summary>
    protected ObjectResult OkWithHeaders(object? value)
    {
        AddRateLimitHeaders();
        return new OkObjectResult(value);
    }

    /// <summary>
    /// Adds rate limiting and correlation headers to the response.
    /// </summary>
    protected void AddRateLimitHeaders()
    {
        // Add correlation ID for request tracking - use incoming header if present
        var correlationId = HttpContext?.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                          ?? HttpContext?.TraceIdentifier 
                          ?? Guid.NewGuid().ToString();
        HttpContext?.Response.Headers.Append("X-Correlation-ID", correlationId);
        
        // Simple rate limiting simulation - in real app would use proper rate limiting middleware
        var remaining = RateLimitMax - 1; // Simulate one request used
        var resetTime = DateTimeOffset.UtcNow.AddMinutes(RateLimitWindowMinutes).ToUnixTimeSeconds();
        
        HttpContext?.Response.Headers.Append("X-RateLimit-Remaining", remaining.ToString());
        HttpContext?.Response.Headers.Append("X-RateLimit-Reset", resetTime.ToString());
    }
}