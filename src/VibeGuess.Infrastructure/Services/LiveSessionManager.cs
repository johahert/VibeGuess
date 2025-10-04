using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VibeGuess.Core.Interfaces;
using VibeGuess.Core.LiveSession;

namespace VibeGuess.Infrastructure.Services;

/// <summary>
/// Redis-based implementation of live session management for real-time quiz gameplay.
/// Handles session state, participants, answers, and scoring in memory with TTL expiration.
/// </summary>
public class LiveSessionManager : ILiveSessionManager
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<LiveSessionManager> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    // Redis key prefixes
    private const string SessionKeyPrefix = "live_session:";
    private const string JoinCodeKeyPrefix = "join_code:";
    private const string ParticipantKeyPrefix = "participant:";
    private const string AnswerKeyPrefix = "answer:";
    
    public LiveSessionManager(IDistributedCache cache, ILogger<LiveSessionManager> logger)
    {
        _cache = cache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }
    
    // Session Management
    public async Task<LiveQuizSession> CreateSessionAsync(string quizId, string title, string hostConnectionId)
    {
        var sessionId = Guid.NewGuid().ToString();
        var joinCode = await GenerateUniqueJoinCodeAsync();
        
        var session = new LiveQuizSession
        {
            SessionId = sessionId,
            JoinCode = joinCode,
            HostConnectionId = hostConnectionId,
            QuizId = quizId,
            Title = title,
            State = LiveSessionState.Lobby,
            CreatedAt = DateTime.UtcNow
        };
        
        var success = await UpdateSessionAsync(session);
        if (!success)
        {
            throw new InvalidOperationException("Failed to create session in Redis");
        }
        
        // Store join code mapping
        await SetJoinCodeMappingAsync(joinCode, sessionId);
        
        _logger.LogInformation("Created live session {SessionId} with join code {JoinCode}", sessionId, joinCode);
        return session;
    }
    
    public async Task<LiveQuizSession?> GetSessionAsync(string sessionId)
    {
        var sessionJson = await _cache.GetStringAsync(GetSessionKey(sessionId));
        if (sessionJson == null)
        {
            return null;
        }
        
        return JsonSerializer.Deserialize<LiveQuizSession>(sessionJson, _jsonOptions);
    }
    
    public async Task<LiveQuizSession?> GetSessionByJoinCodeAsync(string joinCode)
    {
        var sessionId = await GetSessionIdByJoinCodeAsync(joinCode);
        if (sessionId == null)
        {
            return null;
        }
        
        return await GetSessionAsync(sessionId);
    }
    
    public async Task<bool> UpdateSessionAsync(LiveQuizSession session)
    {
        try
        {
            var sessionJson = JsonSerializer.Serialize(session, _jsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = session.GetTTL()
            };
            
            await _cache.SetStringAsync(GetSessionKey(session.SessionId), sessionJson, options);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update session {SessionId}", session.SessionId);
            return false;
        }
    }
    
    public async Task<bool> DeleteSessionAsync(string sessionId)
    {
        try
        {
            // Get session to find join code
            var session = await GetSessionAsync(sessionId);
            if (session != null)
            {
                await _cache.RemoveAsync(GetJoinCodeKey(session.JoinCode));
            }
            
            await _cache.RemoveAsync(GetSessionKey(sessionId));
            _logger.LogInformation("Deleted session {SessionId}", sessionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete session {SessionId}", sessionId);
            return false;
        }
    }
    
    // Join Code Management
    public async Task<string> GenerateUniqueJoinCodeAsync()
    {
        const int maxAttempts = 10;
        var random = new Random();
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var joinCode = GenerateJoinCode(random);
            if (await IsJoinCodeAvailableAsync(joinCode))
            {
                return joinCode;
            }
        }
        
        throw new InvalidOperationException("Failed to generate unique join code after multiple attempts");
    }
    
    public async Task<bool> IsJoinCodeAvailableAsync(string joinCode)
    {
        var sessionId = await _cache.GetStringAsync(GetJoinCodeKey(joinCode));
        return sessionId == null;
    }
    
    // Participant Management
    public async Task<bool> AddParticipantAsync(string sessionId, LiveParticipant participant)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return false;
        }
        
        // Check if participant already exists
        var existingParticipant = session.Participants.FirstOrDefault(p => p.ParticipantId == participant.ParticipantId);
        if (existingParticipant != null)
        {
            // Update existing participant
            session.Participants.Remove(existingParticipant);
        }
        
        session.Participants.Add(participant);
        return await UpdateSessionAsync(session);
    }
    
    public async Task<bool> RemoveParticipantAsync(string sessionId, string participantId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return false;
        }
        
        var participant = session.Participants.FirstOrDefault(p => p.ParticipantId == participantId);
        if (participant == null)
        {
            return false;
        }
        
        session.Participants.Remove(participant);
        return await UpdateSessionAsync(session);
    }
    
    public async Task<bool> UpdateParticipantAsync(string sessionId, LiveParticipant participant)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return false;
        }
        
        var existingParticipant = session.Participants.FirstOrDefault(p => p.ParticipantId == participant.ParticipantId);
        if (existingParticipant == null)
        {
            return false;
        }
        
        var index = session.Participants.IndexOf(existingParticipant);
        session.Participants[index] = participant;
        
        return await UpdateSessionAsync(session);
    }
    
    public async Task<LiveParticipant?> GetParticipantAsync(string sessionId, string participantId)
    {
        var session = await GetSessionAsync(sessionId);
        return session?.Participants.FirstOrDefault(p => p.ParticipantId == participantId);
    }
    
    public async Task<List<LiveParticipant>> GetParticipantsAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        return session?.Participants ?? [];
    }
    
    // Game State Management
    public async Task<bool> StartGameAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null || session.State != LiveSessionState.Lobby)
        {
            return false;
        }
        
        session.State = LiveSessionState.Active;
        session.StartedAt = DateTime.UtcNow;
        session.CurrentQuestionIndex = 0;
        session.QuestionStartTime = DateTime.UtcNow;
        
        // Reset participant question states
        foreach (var participant in session.Participants)
        {
            participant.HasAnsweredCurrentQuestion = false;
            participant.CurrentQuestionAnsweredAt = null;
        }
        
        return await UpdateSessionAsync(session);
    }
    
    public async Task<bool> NextQuestionAsync(string sessionId, int questionIndex, QuestionData questionData)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null || session.State != LiveSessionState.Active)
        {
            _logger.LogWarning("Cannot advance question: Session {SessionId} not found or not active", sessionId);
            return false;
        }
        
        // Validate question data
        if (!questionData.IsValid())
        {
            _logger.LogWarning("Invalid question data provided for session {SessionId}, question {QuestionIndex}", sessionId, questionIndex);
            return false;
        }
        
        // Validate question index progression
        if (questionIndex < 0 || questionIndex < session.CurrentQuestionIndex)
        {
            _logger.LogWarning("Invalid question index {QuestionIndex} for session {SessionId}. Current index: {CurrentIndex}", 
                questionIndex, sessionId, session.CurrentQuestionIndex);
            return false;
        }
        
        _logger.LogInformation("Advancing session {SessionId} to question {QuestionIndex}: {QuestionText}", 
            sessionId, questionIndex, questionData.QuestionText);
        
        // Update session state
        session.CurrentQuestionIndex = questionIndex;
        session.QuestionStartTime = DateTime.UtcNow;
        session.CurrentQuestion = questionData;
        
        // Override session time limit if question specifies one
        if (questionData.TimeLimit.HasValue)
        {
            session.QuestionTimeLimit = questionData.TimeLimit.Value;
        }
        
        // Reset participant question states for new question
        foreach (var participant in session.Participants)
        {
            participant.HasAnsweredCurrentQuestion = false;
            participant.CurrentQuestionAnsweredAt = null;
        }
        
        return await UpdateSessionAsync(session);
    }
    
    public async Task<QuestionData?> GetCurrentQuestionAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        return session?.CurrentQuestion;
    }
    
    public async Task<bool> EndGameAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return false;
        }
        
        session.State = LiveSessionState.Completed;
        session.EndedAt = DateTime.UtcNow;
        
        return await UpdateSessionAsync(session);
    }
    
    public async Task<bool> PauseGameAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null || session.State != LiveSessionState.Active)
        {
            return false;
        }
        
        session.State = LiveSessionState.Paused;
        return await UpdateSessionAsync(session);
    }
    
    public async Task<bool> ResumeGameAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null || session.State != LiveSessionState.Paused)
        {
            return false;
        }
        
        session.State = LiveSessionState.Active;
        return await UpdateSessionAsync(session);
    }
    
    // Answer Management
    public async Task<LiveAnswer> SubmitAnswerAsync(string sessionId, string participantId, int questionIndex, string selectedAnswer)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Session not found");
        }
        
        var participant = session.Participants.FirstOrDefault(p => p.ParticipantId == participantId);
        if (participant == null)
        {
            throw new InvalidOperationException("Participant not found");
        }
        
        // Validate that this is for the current question
        if (questionIndex != session.CurrentQuestionIndex)
        {
            throw new InvalidOperationException("Can only submit answers for the current question");
        }
        
        // Get correct answer from current question
        var correctAnswer = session.CurrentQuestion?.CorrectAnswer;
        if (string.IsNullOrEmpty(correctAnswer))
        {
            throw new InvalidOperationException("No current question or correct answer not set");
        }
        
        // Validate selected answer is one of the options
        if (session.CurrentQuestion?.Options != null && 
            !session.CurrentQuestion.Options.Contains(selectedAnswer, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Selected answer is not a valid option");
        }
        
        // Calculate response time from question start
        var responseTime = session.QuestionStartTime.HasValue 
            ? DateTime.UtcNow - session.QuestionStartTime.Value 
            : TimeSpan.Zero;
        
        var answer = new LiveAnswer
        {
            AnswerId = Guid.NewGuid().ToString(),
            SessionId = sessionId,
            ParticipantId = participantId,
            QuestionIndex = questionIndex,
            SelectedAnswer = selectedAnswer,
            IsCorrect = selectedAnswer.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase),
            ResponseTime = responseTime,
            SubmittedAt = DateTime.UtcNow
        };
        
        // Calculate scoring with question-specific points if available
        var questionPoints = session.CurrentQuestion?.Points ?? 100; // Default 100 points
        answer.CalculateTimeBonus(session.QuestionTimeLimit, questionPoints);
        
        // Update participant state
        participant.HasAnsweredCurrentQuestion = true;
        participant.CurrentQuestionAnsweredAt = DateTime.UtcNow;
        participant.TotalAnswers++;
        participant.UpdateActivity();
        
        if (answer.IsCorrect)
        {
            participant.CorrectAnswers++;
            participant.Score += answer.TotalScore;
        }
        
        // Add answer to session
        session.Answers.Add(answer);
        
        await UpdateSessionAsync(session);
        
        _logger.LogInformation("Answer submitted for session {SessionId}, participant {ParticipantId}, question {QuestionIndex}, correct: {IsCorrect}", 
            sessionId, participantId, questionIndex, answer.IsCorrect);
        
        return answer;
    }
    
    public async Task<List<LiveAnswer>> GetAnswersForQuestionAsync(string sessionId, int questionIndex)
    {
        var session = await GetSessionAsync(sessionId);
        return session?.Answers.Where(a => a.QuestionIndex == questionIndex).ToList() ?? [];
    }
    
    public async Task<List<LiveAnswer>> GetParticipantAnswersAsync(string sessionId, string participantId)
    {
        var session = await GetSessionAsync(sessionId);
        return session?.Answers.Where(a => a.ParticipantId == participantId).ToList() ?? [];
    }
    
    // Scoring and Leaderboard
    public async Task<List<LiveParticipant>> GetLeaderboardAsync(string sessionId)
    {
        var participants = await GetParticipantsAsync(sessionId);
        return participants
            .OrderByDescending(p => p.Score)
            .ThenByDescending(p => p.CorrectAnswers)
            .ThenBy(p => p.JoinedAt)
            .ToList();
    }
    
    public async Task UpdateScoresAsync(string sessionId, int questionIndex)
    {
        // Scores are updated in real-time when answers are submitted
        // This method could be used for batch score recalculations if needed
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return;
        }
        
        _logger.LogInformation("Scores updated for session {SessionId}, question {QuestionIndex}", sessionId, questionIndex);
    }
    
    // Session Cleanup
    public Task CleanupExpiredSessionsAsync()
    {
        // Redis handles TTL expiration automatically
        // This method could be used for additional cleanup logic
        _logger.LogInformation("Session cleanup completed (Redis TTL handles automatic expiration)");
        return Task.CompletedTask;
    }
    
    public async Task<bool> ExtendSessionTTLAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return false;
        }
        
        return await UpdateSessionAsync(session); // This will reset the TTL
    }
    
    // Connection Management
    public async Task<bool> UpdateParticipantConnectionAsync(string sessionId, string participantId, string newConnectionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return false;
        }
        
        var participant = session.Participants.FirstOrDefault(p => p.ParticipantId == participantId);
        if (participant == null)
        {
            return false;
        }
        
        participant.ConnectionId = newConnectionId;
        participant.IsConnected = true;
        participant.UpdateActivity();
        
        return await UpdateSessionAsync(session);
    }
    
    public async Task<bool> MarkParticipantDisconnectedAsync(string sessionId, string participantId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return false;
        }
        
        var participant = session.Participants.FirstOrDefault(p => p.ParticipantId == participantId);
        if (participant == null)
        {
            return false;
        }
        
        participant.IsConnected = false;
        participant.UpdateActivity();
        
        return await UpdateSessionAsync(session);
    }
    
    public async Task<List<string>> GetDisconnectedParticipantsAsync(string sessionId, TimeSpan gracePeriod)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return [];
        }
        
        var cutoffTime = DateTime.UtcNow - gracePeriod;
        return session.Participants
            .Where(p => !p.IsConnected && p.LastActivity < cutoffTime)
            .Select(p => p.ParticipantId)
            .ToList();
    }
    
    // Private Helper Methods
    private static string GetSessionKey(string sessionId) => $"{SessionKeyPrefix}{sessionId}";
    private static string GetJoinCodeKey(string joinCode) => $"{JoinCodeKeyPrefix}{joinCode}";
    
    private async Task SetJoinCodeMappingAsync(string joinCode, string sessionId)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(4) // Join codes expire after 4 hours
        };
        
        await _cache.SetStringAsync(GetJoinCodeKey(joinCode), sessionId, options);
    }
    
    private async Task<string?> GetSessionIdByJoinCodeAsync(string joinCode)
    {
        return await _cache.GetStringAsync(GetJoinCodeKey(joinCode));
    }
    
    private static string GenerateJoinCode(Random random)
    {
        // Generate a 6-character alphanumeric code (excluding confusing characters like 0, O, I, 1)
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}