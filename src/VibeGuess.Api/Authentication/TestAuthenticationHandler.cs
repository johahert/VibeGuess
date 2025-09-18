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
            return Task.FromResult(AuthenticateResult.Fail("Token expired"));
        }
        
        if (token.Contains("invalid") && !token.Contains("InvalidSpotify"))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
        }

        // Handle tokens that are valid JWT but invalid for Spotify (should result in 403)
        if (token.Contains("InvalidSpotify") || token.Contains("ButInvalidSpotify"))
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
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim("spotify_user_id", "spotify123")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}