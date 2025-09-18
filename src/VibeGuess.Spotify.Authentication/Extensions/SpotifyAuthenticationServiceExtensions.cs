using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VibeGuess.Spotify.Authentication.Models;
using VibeGuess.Spotify.Authentication.Services;

namespace VibeGuess.Spotify.Authentication.Extensions;

/// <summary>
/// Extension methods for configuring Spotify authentication services.
/// </summary>
public static class SpotifyAuthenticationServiceExtensions
{
    /// <summary>
    /// Adds Spotify authentication services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSpotifyAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure options
        services.Configure<SpotifyAuthenticationOptions>(
            configuration.GetSection(SpotifyAuthenticationOptions.SectionName));

        // Register HTTP client for Spotify API calls
        services.AddHttpClient<ISpotifyAuthenticationService, SpotifyAuthenticationService>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "VibeGuess/1.0");
        });

        // Register authentication service
        services.AddScoped<ISpotifyAuthenticationService, SpotifyAuthenticationService>();

        return services;
    }
}