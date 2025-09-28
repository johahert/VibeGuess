using VibeGuess.Core.LiveSession;

namespace VibeGuess.Core.Interfaces;

/// <summary>
/// Service for managing live quiz sessions in Redis cache.
/// Handles session lifecycle, participant management, and real-time state updates.
/// </summary>
public interface ILiveSessionManager
{
    // Session Management
    Task<LiveQuizSession> CreateSessionAsync(string quizId, string title, string hostConnectionId);
    Task<LiveQuizSession?> GetSessionAsync(string sessionId);
    Task<LiveQuizSession?> GetSessionByJoinCodeAsync(string joinCode);
    Task<bool> UpdateSessionAsync(LiveQuizSession session);
    Task<bool> DeleteSessionAsync(string sessionId);
    
    // Join Code Management
    Task<string> GenerateUniqueJoinCodeAsync();
    Task<bool> IsJoinCodeAvailableAsync(string joinCode);
    
    // Participant Management
    Task<bool> AddParticipantAsync(string sessionId, LiveParticipant participant);
    Task<bool> RemoveParticipantAsync(string sessionId, string participantId);
    Task<bool> UpdateParticipantAsync(string sessionId, LiveParticipant participant);
    Task<LiveParticipant?> GetParticipantAsync(string sessionId, string participantId);
    Task<List<LiveParticipant>> GetParticipantsAsync(string sessionId);
    
    // Game State Management
    Task<bool> StartGameAsync(string sessionId);
    Task<bool> NextQuestionAsync(string sessionId, int questionIndex);
    Task<bool> EndGameAsync(string sessionId);
    Task<bool> PauseGameAsync(string sessionId);
    Task<bool> ResumeGameAsync(string sessionId);
    
    // Answer Management
    Task<LiveAnswer> SubmitAnswerAsync(string sessionId, string participantId, int questionIndex, string selectedAnswer, string correctAnswer);
    Task<List<LiveAnswer>> GetAnswersForQuestionAsync(string sessionId, int questionIndex);
    Task<List<LiveAnswer>> GetParticipantAnswersAsync(string sessionId, string participantId);
    
    // Scoring and Leaderboard
    Task<List<LiveParticipant>> GetLeaderboardAsync(string sessionId);
    Task UpdateScoresAsync(string sessionId, int questionIndex);
    
    // Session Cleanup
    Task CleanupExpiredSessionsAsync();
    Task<bool> ExtendSessionTTLAsync(string sessionId);
    
    // Connection Management
    Task<bool> UpdateParticipantConnectionAsync(string sessionId, string participantId, string newConnectionId);
    Task<bool> MarkParticipantDisconnectedAsync(string sessionId, string participantId);
    Task<List<string>> GetDisconnectedParticipantsAsync(string sessionId, TimeSpan gracePeriod);
}