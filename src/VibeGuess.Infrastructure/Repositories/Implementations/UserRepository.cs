using Microsoft.EntityFrameworkCore;
using VibeGuess.Core.Entities;
using VibeGuess.Infrastructure.Data;
using VibeGuess.Infrastructure.Repositories.Interfaces;

namespace VibeGuess.Infrastructure.Repositories.Implementations;

/// <summary>
/// Repository implementation for User entity operations.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(VibeGuessDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<User?> GetBySpotifyUserIdAsync(string spotifyUserId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(spotifyUserId);

        return await _dbSet
            .FirstOrDefaultAsync(u => u.SpotifyUserId == spotifyUserId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetByCountryAsync(string country, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(country);

        return await _dbSet
            .Where(u => u.Country == country)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetPremiumUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.HasSpotifyPremium)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsBySpotifyUserIdAsync(string spotifyUserId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(spotifyUserId);

        return await _dbSet
            .AnyAsync(u => u.SpotifyUserId == spotifyUserId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetWithSettingsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Settings)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetWithTokensAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.SpotifyTokens)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SpotifyToken>> GetUserSpotifyTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<SpotifyToken>()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}