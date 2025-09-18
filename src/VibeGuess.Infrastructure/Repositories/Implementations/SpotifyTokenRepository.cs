using Microsoft.EntityFrameworkCore;
using VibeGuess.Core.Entities;
using VibeGuess.Infrastructure.Data;
using VibeGuess.Infrastructure.Repositories.Interfaces;

namespace VibeGuess.Infrastructure.Repositories.Implementations;

/// <summary>
/// Repository implementation for SpotifyToken entity operations.
/// </summary>
public class SpotifyTokenRepository : Repository<SpotifyToken>, ISpotifyTokenRepository
{
    public SpotifyTokenRepository(VibeGuessDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SpotifyToken>> GetActiveTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.UserId == userId && t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeactivateUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _dbSet
            .Where(t => t.UserId == userId && t.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.IsActive = false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SpotifyToken>> GetTokensNeedingRefreshAsync(int minutesBeforeExpiry = 5, CancellationToken cancellationToken = default)
    {
        var refreshThreshold = DateTime.UtcNow.AddMinutes(minutesBeforeExpiry);

        return await _dbSet
            .Where(t => t.IsActive && t.ExpiresAt <= refreshThreshold)
            .ToListAsync(cancellationToken);
    }
}