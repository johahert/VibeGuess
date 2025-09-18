using Microsoft.EntityFrameworkCore;
using VibeGuess.Core.Entities;
using VibeGuess.Infrastructure.Data;
using VibeGuess.Infrastructure.Repositories.Interfaces;

namespace VibeGuess.Infrastructure.Repositories.Implementations;

/// <summary>
/// Repository implementation for QuizSession entity operations.
/// </summary>
public class QuizSessionRepository : Repository<QuizSession>, IQuizSessionRepository
{
    public QuizSessionRepository(VibeGuessDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QuizSession>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(qs => qs.UserId == userId)
            .OrderByDescending(qs => qs.StartedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QuizSession>> GetByQuizIdAsync(Guid quizId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(qs => qs.QuizId == quizId)
            .OrderByDescending(qs => qs.StartedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QuizSession>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(status);

        return await _dbSet
            .Where(qs => qs.Status == status)
            .OrderByDescending(qs => qs.StartedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QuizSession>> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(qs => qs.UserId == userId && qs.Status == "InProgress")
            .OrderByDescending(qs => qs.StartedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QuizSession>> GetCompletedSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(qs => qs.UserId == userId && qs.Status == "Completed")
            .OrderByDescending(qs => qs.CompletedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<QuizSession?> GetWithAnswersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(qs => qs.UserAnswers)
                .ThenInclude(ua => ua.Question)
            .Include(qs => qs.Quiz)
            .FirstOrDefaultAsync(qs => qs.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QuizSession>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(qs => qs.StartedAt >= startDate && qs.StartedAt <= endDate)
            .OrderByDescending(qs => qs.StartedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int?> GetBestScoreAsync(Guid userId, Guid quizId, CancellationToken cancellationToken = default)
    {
        var bestSession = await _dbSet
            .Where(qs => qs.UserId == userId && qs.QuizId == quizId && qs.Status == "Completed")
            .OrderByDescending(qs => qs.CurrentScore)
            .FirstOrDefaultAsync(cancellationToken);

        return bestSession?.CurrentScore;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QuizSession>> GetLeaderboardAsync(Guid quizId, int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(qs => qs.QuizId == quizId && qs.Status == "Completed")
            .OrderByDescending(qs => qs.CurrentScore)
            .ThenBy(qs => qs.CompletedAt) // Earlier completion time wins in case of tie
            .Take(limit)
            .Include(qs => qs.User)
            .ToListAsync(cancellationToken);
    }
}