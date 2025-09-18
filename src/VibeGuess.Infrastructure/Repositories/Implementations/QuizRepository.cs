using Microsoft.EntityFrameworkCore;
using VibeGuess.Core.Entities;
using VibeGuess.Infrastructure.Data;
using VibeGuess.Infrastructure.Repositories.Interfaces;

namespace VibeGuess.Infrastructure.Repositories.Implementations;

/// <summary>
/// Repository implementation for Quiz entity operations.
/// </summary>
public class QuizRepository : Repository<Quiz>, IQuizRepository
{
    public QuizRepository(VibeGuessDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Quiz>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Quiz>> GetByDifficultyAsync(string difficulty, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(difficulty);

        return await _dbSet
            .Where(q => q.Difficulty == difficulty)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Quiz>> GetByGenreAsync(string genre, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(genre);

        return await _dbSet
            .Where(q => q.Tags.Contains(genre))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Quiz>> GetByTimeRangeAsync(string timeRangeStart, string timeRangeEnd, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeRangeStart);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeRangeEnd);

        return await _dbSet
            .Where(q => q.Tags.Contains(timeRangeStart) && q.Tags.Contains(timeRangeEnd))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Quiz?> GetWithQuestionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(q => q.Questions)
                .ThenInclude(question => question.AnswerOptions)
            .Include(q => q.Questions)
                .ThenInclude(question => question.Track)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Quiz?> GetWithMetadataAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(q => q.GenerationMetadata)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Quiz>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchTerm);

        var term = searchTerm.ToLower();
        return await _dbSet
            .Where(q => q.Title.ToLower().Contains(term) || 
                       q.UserPrompt.ToLower().Contains(term) ||
                       q.Tags.ToLower().Contains(term))
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Quiz>> GetRecentAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderByDescending(q => q.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}