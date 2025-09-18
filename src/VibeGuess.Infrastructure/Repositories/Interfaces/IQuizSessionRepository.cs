using VibeGuess.Core.Entities;

namespace VibeGuess.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface for QuizSession entity operations.
/// </summary>
public interface IQuizSessionRepository : IRepository<QuizSession>
{
    /// <summary>
    /// Gets quiz sessions for a specific user.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of quiz sessions for the user</returns>
    Task<IEnumerable<QuizSession>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quiz sessions for a specific quiz.
    /// </summary>
    /// <param name="quizId">The quiz ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of quiz sessions for the quiz</returns>
    Task<IEnumerable<QuizSession>> GetByQuizIdAsync(Guid quizId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quiz sessions by status.
    /// </summary>
    /// <param name="status">The session status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of quiz sessions with the specified status</returns>
    Task<IEnumerable<QuizSession>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active (in-progress) quiz sessions for a user.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active quiz sessions</returns>
    Task<IEnumerable<QuizSession>> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets completed quiz sessions for a user.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of completed quiz sessions</returns>
    Task<IEnumerable<QuizSession>> GetCompletedSessionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a quiz session with user answers included.
    /// </summary>
    /// <param name="id">The quiz session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The quiz session with answers or null if not found</returns>
    Task<QuizSession?> GetWithAnswersAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quiz sessions within a date range.
    /// </summary>
    /// <param name="startDate">The start date</param>
    /// <param name="endDate">The end date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of quiz sessions within the date range</returns>
    Task<IEnumerable<QuizSession>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user's best score for a specific quiz.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="quizId">The quiz ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The highest score achieved by the user for the quiz</returns>
    Task<int?> GetBestScoreAsync(Guid userId, Guid quizId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets leaderboard data for a quiz (top scores).
    /// </summary>
    /// <param name="quizId">The quiz ID</param>
    /// <param name="limit">Maximum number of entries to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of top quiz sessions ordered by score</returns>
    Task<IEnumerable<QuizSession>> GetLeaderboardAsync(Guid quizId, int limit = 10, CancellationToken cancellationToken = default);
}