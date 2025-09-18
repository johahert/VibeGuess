using VibeGuess.Core.Entities;

namespace VibeGuess.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface for Quiz entity operations.
/// </summary>
public interface IQuizRepository : IRepository<Quiz>
{
    /// <summary>
    /// Gets quizzes created by a specific user.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of quizzes created by the user</returns>
    Task<IEnumerable<Quiz>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quizzes by difficulty level.
    /// </summary>
    /// <param name="difficulty">The difficulty level</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of quizzes with the specified difficulty</returns>
    Task<IEnumerable<Quiz>> GetByDifficultyAsync(string difficulty, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quizzes by genre.
    /// </summary>
    /// <param name="genre">The genre</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of quizzes with the specified genre</returns>
    Task<IEnumerable<Quiz>> GetByGenreAsync(string genre, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quizzes within a specific time range.
    /// </summary>
    /// <param name="timeRangeStart">The minimum time range</param>
    /// <param name="timeRangeEnd">The maximum time range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of quizzes within the time range</returns>
    Task<IEnumerable<Quiz>> GetByTimeRangeAsync(string timeRangeStart, string timeRangeEnd, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a quiz with its questions and answer options included.
    /// </summary>
    /// <param name="id">The quiz ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The quiz with questions and options or null if not found</returns>
    Task<Quiz?> GetWithQuestionsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a quiz with its generation metadata included.
    /// </summary>
    /// <param name="id">The quiz ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The quiz with generation metadata or null if not found</returns>
    Task<Quiz?> GetWithMetadataAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches quizzes by title or description.
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching quizzes</returns>
    Task<IEnumerable<Quiz>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent quizzes (ordered by creation date).
    /// </summary>
    /// <param name="count">Maximum number of quizzes to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of recent quizzes</returns>
    Task<IEnumerable<Quiz>> GetRecentAsync(int count = 10, CancellationToken cancellationToken = default);
}