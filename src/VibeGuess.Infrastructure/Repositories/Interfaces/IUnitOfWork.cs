using VibeGuess.Infrastructure.Repositories.Interfaces;

namespace VibeGuess.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Unit of work interface for managing transactions across multiple repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the user repository.
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// Gets the quiz repository.
    /// </summary>
    IQuizRepository Quizzes { get; }

    /// <summary>
    /// Gets the quiz session repository.
    /// </summary>
    IQuizSessionRepository QuizSessions { get; }

    /// <summary>
    /// Gets the track repository.
    /// </summary>
    ITrackRepository Tracks { get; }

    /// <summary>
    /// Gets the Spotify token repository.
    /// </summary>
    ISpotifyTokenRepository SpotifyTokens { get; }

    /// <summary>
    /// Saves all changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of entities written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}