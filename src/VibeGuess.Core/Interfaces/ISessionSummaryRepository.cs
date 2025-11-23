using VibeGuess.Core.Entities;

namespace VibeGuess.Core.Interfaces;

/// <summary>
/// Repository for optional session analytics persistence.
/// Used only for storing completed session summaries, not for live session data.
/// </summary>
public interface ISessionSummaryRepository
{
    Task<SessionSummary?> GetByIdAsync(int id);
    Task<SessionSummary?> GetBySessionIdAsync(string sessionId);
    Task<List<SessionSummary>> GetRecentSessionsAsync(int count = 50);
    Task<List<SessionSummary>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<SessionSummary> CreateAsync(SessionSummary sessionSummary);
    Task<SessionSummary> UpdateAsync(SessionSummary sessionSummary);
    Task<bool> DeleteAsync(int id);
    
    // Analytics queries
    Task<double> GetAverageSessionDurationAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<double> GetAverageParticipantCountAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<SessionSummary>> GetTopSessionsByScoreAsync(int count = 10);
}