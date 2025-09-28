using Microsoft.Extensions.DependencyInjection;
using VibeGuess.Infrastructure.Repositories.Interfaces;
using VibeGuess.Infrastructure.Repositories.Implementations;

namespace VibeGuess.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring repository dependencies.
/// </summary>
public static class RepositoryServiceExtensions
{
    /// <summary>
    /// Adds repository services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IQuizSessionRepository, QuizSessionRepository>();
        services.AddScoped<ITrackRepository, TrackRepository>();
        services.AddScoped<ISpotifyTokenRepository, SpotifyTokenRepository>();
        services.AddScoped<VibeGuess.Core.Interfaces.ISessionSummaryRepository, VibeGuess.Infrastructure.Repositories.SessionSummaryRepository>();

        // Register unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}