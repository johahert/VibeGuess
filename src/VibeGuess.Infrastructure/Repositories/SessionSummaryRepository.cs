using Microsoft.EntityFrameworkCore;
using VibeGuess.Core.Entities;
using VibeGuess.Core.Interfaces;
using VibeGuess.Infrastructure.Data;

namespace VibeGuess.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for SessionSummary analytics persistence.
/// Handles optional storage of completed session data for historical analysis.
/// </summary>
public class SessionSummaryRepository : ISessionSummaryRepository
{
    private readonly VibeGuessDbContext _context;

    public SessionSummaryRepository(VibeGuessDbContext context)
    {
        _context = context;
    }

    public async Task<SessionSummary?> GetByIdAsync(int id)
    {
        return await _context.SessionSummaries.FindAsync(id);
    }

    public async Task<SessionSummary?> GetBySessionIdAsync(string sessionId)
    {
        return await _context.SessionSummaries
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task<List<SessionSummary>> GetRecentSessionsAsync(int count = 50)
    {
        return await _context.SessionSummaries
            .OrderByDescending(s => s.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<SessionSummary>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.SessionSummaries
            .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<SessionSummary> CreateAsync(SessionSummary sessionSummary)
    {
        _context.SessionSummaries.Add(sessionSummary);
        await _context.SaveChangesAsync();
        return sessionSummary;
    }

    public async Task<SessionSummary> UpdateAsync(SessionSummary sessionSummary)
    {
        _context.SessionSummaries.Update(sessionSummary);
        await _context.SaveChangesAsync();
        return sessionSummary;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var sessionSummary = await GetByIdAsync(id);
        if (sessionSummary == null)
        {
            return false;
        }

        _context.SessionSummaries.Remove(sessionSummary);
        await _context.SaveChangesAsync();
        return true;
    }

    // Analytics queries
    public async Task<double> GetAverageSessionDurationAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.SessionSummaries
            .Where(s => s.StartedAt.HasValue && s.EndedAt.HasValue);

        if (startDate.HasValue)
        {
            query = query.Where(s => s.CreatedAt >= startDate);
        }

        if (endDate.HasValue)
        {
            query = query.Where(s => s.CreatedAt <= endDate);
        }

        var sessions = await query.ToListAsync();
        
        if (!sessions.Any())
        {
            return 0;
        }

        var durations = sessions
            .Where(s => s.Duration.HasValue)
            .Select(s => s.Duration!.Value.TotalMinutes);

        return durations.Any() ? durations.Average() : 0;
    }

    public async Task<double> GetAverageParticipantCountAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.SessionSummaries.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(s => s.CreatedAt >= startDate);
        }

        if (endDate.HasValue)
        {
            query = query.Where(s => s.CreatedAt <= endDate);
        }

        return await query.AverageAsync(s => (double)s.ParticipantCount);
    }

    public async Task<List<SessionSummary>> GetTopSessionsByScoreAsync(int count = 10)
    {
        return await _context.SessionSummaries
            .OrderByDescending(s => s.AverageScore)
            .Take(count)
            .ToListAsync();
    }
}