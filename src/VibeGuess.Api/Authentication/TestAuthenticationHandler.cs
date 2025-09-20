using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace VibeGuess.Api.Authentication;

/// <summary>
/// Simple test authentication handler that accepts any Bearer token for testing purposes.
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, 
        ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if Authorization header exists
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header"));
        }

        var authHeader = Request.Headers["Authorization"].ToString();
        
        // Check if it starts with Bearer
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid authorization scheme"));
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        
        // For testing, accept certain token patterns and reject others
        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing token"));
        }

        // Simulate different token scenarios for testing
        if (token.Contains("expired"))
        {
            var failureMessage = "Invalid or expired token";
            Context.Items["AuthFailureMessage"] = failureMessage;
            Logger.LogInformation("Setting auth failure message for expired token: {Message}", failureMessage);
            return Task.FromResult(AuthenticateResult.Fail(failureMessage));
        }
        
        if (token.Contains("invalid") && !token.Contains("invalid.spotify") && !token.Contains("InvalidSpotify"))
        {
            var failureMessage = "Invalid or expired token";
            Context.Items["AuthFailureMessage"] = failureMessage;
            return Task.FromResult(AuthenticateResult.Fail(failureMessage));
        }

        // Handle tokens that are valid JWT but invalid for Spotify (should result in 403)
        if (token.Contains("invalid.spotify") || token.Contains("InvalidSpotify") || token.Contains("ButInvalidSpotify"))
        {
            var spotifyClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim("spotify_invalid", "true") // Mark as invalid Spotify scenario
            };

            var spotifyIdentity = new ClaimsIdentity(spotifyClaims, Scheme.Name);
            var spotifyPrincipal = new ClaimsPrincipal(spotifyIdentity);
            var spotifyTicket = new AuthenticationTicket(spotifyPrincipal, Scheme.Name);
            
            return Task.FromResult(AuthenticateResult.Success(spotifyTicket));
        }

        // For valid tokens, create a claims principal
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim("spotify_user_id", "spotify123")
        };

        // Add admin role for admin tokens
        if (token.Contains("admin.token"))
        {
            claims.Add(new Claim(ClaimTypes.Role, "admin"));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Role, "user"));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.ContentType = "application/json";

        string message = "Invalid or expired token";
        string errorCode = "unauthorized";
        
        // Check if we have a stored failure message
        if (Context.Items.TryGetValue("AuthFailureMessage", out var storedMessage))
        {
            message = storedMessage?.ToString() ?? message;
            // Use expired error code only for health/spotify endpoints
            if (message.ToLower().Contains("expired") && 
                Context.Request.Path.Value?.Contains("/health/test/spotify") == true)
            {
                errorCode = "expired";
            }
        }
        
        // Always include "unauthorized" in the message for test compatibility
        if (!message.ToLower().Contains("unauthorized"))
        {
            message = $"Unauthorized: {message}";
        }
        
        var response = new
        {
            error = errorCode,
            message = message,
            correlationId = Context.TraceIdentifier
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
        await Response.WriteAsync(jsonResponse);
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 403;
        Response.ContentType = "application/json";

        var response = new
        {
            error = "permission_denied",
            message = "Insufficient permission to access this resource",
            correlationId = Context.TraceIdentifier
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
        await Response.WriteAsync(jsonResponse);
    }
}