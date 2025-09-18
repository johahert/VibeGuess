namespace VibeGuess.Api.Models;

/// <summary>
/// Standard error response format for API errors.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error code identifying the type of error.
    /// </summary>
    public required string Error { get; set; }
    
    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public required string Message { get; set; }
    
    /// <summary>
    /// Correlation ID for tracking this request.
    /// </summary>
    public required string CorrelationId { get; set; }
}