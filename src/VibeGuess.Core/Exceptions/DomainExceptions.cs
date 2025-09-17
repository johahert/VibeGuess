namespace VibeGuess.Core.Exceptions;

/// <summary>
/// Base exception for all VibeGuess domain exceptions.
/// </summary>
public abstract class VibeGuessException : Exception
{
    /// <summary>
    /// Error code for this exception.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Correlation ID for tracking this error.
    /// </summary>
    public string? CorrelationId { get; set; }

    protected VibeGuessException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    protected VibeGuessException(string errorCode, string message, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception thrown when a requested entity is not found.
/// </summary>
public class EntityNotFoundException : VibeGuessException
{
    public EntityNotFoundException(string entityType, Guid id)
        : base("entity_not_found", $"{entityType} with ID {id} was not found.")
    {
    }

    public EntityNotFoundException(string entityType, string identifier)
        : base("entity_not_found", $"{entityType} with identifier '{identifier}' was not found.")
    {
    }
}

/// <summary>
/// Exception thrown when a request contains invalid data.
/// </summary>
public class InvalidRequestException : VibeGuessException
{
    public List<string> ValidationErrors { get; }

    public InvalidRequestException(string message) 
        : base("invalid_request", message)
    {
        ValidationErrors = new List<string>();
    }

    public InvalidRequestException(List<string> validationErrors)
        : base("invalid_request", "The request contains validation errors.")
    {
        ValidationErrors = validationErrors;
    }

    public InvalidRequestException(string message, List<string> validationErrors)
        : base("invalid_request", message)
    {
        ValidationErrors = validationErrors;
    }
}

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleException : VibeGuessException
{
    public BusinessRuleException(string message)
        : base("business_rule_violation", message)
    {
    }

    public BusinessRuleException(string message, Exception innerException)
        : base("business_rule_violation", message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when authentication fails.
/// </summary>
public class AuthenticationException : VibeGuessException
{
    public AuthenticationException(string message)
        : base("unauthorized", message)
    {
    }

    public AuthenticationException()
        : base("unauthorized", "Invalid or expired token")
    {
    }
}

/// <summary>
/// Exception thrown when authorization fails.
/// </summary>
public class AuthorizationException : VibeGuessException
{
    public AuthorizationException(string message)
        : base("forbidden", message)
    {
    }

    public AuthorizationException()
        : base("forbidden", "Access denied. Insufficient permissions.")
    {
    }
}

/// <summary>
/// Exception thrown when an operation conflicts with current state.
/// </summary>
public class ConflictException : VibeGuessException
{
    public ConflictException(string message)
        : base("conflict", message)
    {
    }
}

/// <summary>
/// Exception thrown when quiz generation fails.
/// </summary>
public class QuizGenerationException : VibeGuessException
{
    public List<string>? SuggestedPrompts { get; set; }

    public QuizGenerationException(string message)
        : base("content_generation_failed", message)
    {
    }

    public QuizGenerationException(string message, List<string> suggestedPrompts)
        : base("content_generation_failed", message)
    {
        SuggestedPrompts = suggestedPrompts;
    }

    public QuizGenerationException(string message, Exception innerException)
        : base("content_generation_failed", message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when external service operations fail.
/// </summary>
public class ExternalServiceException : VibeGuessException
{
    public string ServiceName { get; }

    public ExternalServiceException(string serviceName, string message)
        : base("external_service_error", $"{serviceName}: {message}")
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException)
        : base("external_service_error", $"{serviceName}: {message}", innerException)
    {
        ServiceName = serviceName;
    }
}

/// <summary>
/// Exception thrown when rate limits are exceeded.
/// </summary>
public class RateLimitExceededException : VibeGuessException
{
    public TimeSpan RetryAfter { get; }

    public RateLimitExceededException(TimeSpan retryAfter)
        : base("rate_limit_exceeded", $"Rate limit exceeded. Retry after {retryAfter.TotalSeconds} seconds.")
    {
        RetryAfter = retryAfter;
    }

    public RateLimitExceededException(string message, TimeSpan retryAfter)
        : base("rate_limit_exceeded", message)
    {
        RetryAfter = retryAfter;
    }
}