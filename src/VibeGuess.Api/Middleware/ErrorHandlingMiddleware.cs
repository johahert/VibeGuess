using System.Text.Json;

namespace VibeGuess.Api.Middleware;

/// <summary>
/// Middleware to handle authentication and authorization errors with JSON responses.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }

        // Handle authentication/authorization responses
        if (context.Response.StatusCode == 401 && !context.Response.HasStarted)
        {
            await HandleUnauthorizedAsync(context);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "internal_error",
            message = "An internal server error occurred",
            correlationId = context.TraceIdentifier
        };

        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }

    private static async Task HandleUnauthorizedAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "unauthorized",
            message = "Authentication is required to access this resource",
            correlationId = context.TraceIdentifier
        };

        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }
}