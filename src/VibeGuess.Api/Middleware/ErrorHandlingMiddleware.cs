using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

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
            string failureMessage = "Authentication is required to access this resource";
            
            // Check if we have stored authentication failure information
            if (context.Items.TryGetValue("AuthFailureMessage", out var storedMessage))
            {
                failureMessage = storedMessage?.ToString() ?? failureMessage;
                _logger.LogInformation("Using stored auth failure message: {Message}", failureMessage);
            }
            else
            {
                _logger.LogInformation("No stored auth failure message found, using default");
            }
            
            await HandleUnauthorizedAsync(context, failureMessage);
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

    private static async Task HandleUnauthorizedAsync(HttpContext context, string message = "Authentication is required to access this resource")
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "unauthorized",
            message = message,
            correlationId = context.TraceIdentifier
        };

        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }
}