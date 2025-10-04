using System.Text.Json.Serialization;

namespace VibeGuess.Core.LiveSession;

/// <summary>
/// In-memory live quiz session stored in Redis cache.
/// NOT a database entity - used only for real-time gameplay state.
/// </summary>
public class LiveQuizSession
{
    public required string SessionId { get; set; }
    public required string JoinCode { get; set; }
    public required string HostConnectionId { get; set; }
    public required string QuizId { get; set; }
    public required string Title { get; set; }
    public LiveSessionState State { get; set; } = LiveSessionState.Lobby;
    
    public int CurrentQuestionIndex { get; set; } = 0;
    public DateTime? QuestionStartTime { get; set; }
    public int QuestionTimeLimit { get; set; } = 30; // seconds
    public QuestionData? CurrentQuestion { get; set; }
    
    public List<LiveParticipant> Participants { get; set; } = [];
    public List<LiveAnswer> Answers { get; set; } = [];
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    
    // Redis TTL management
    public TimeSpan GetTTL()
    {
        return State switch
        {
            LiveSessionState.Lobby => TimeSpan.FromHours(2), // 2 hours in lobby
            LiveSessionState.Active => TimeSpan.FromHours(1), // 1 hour during gameplay
            LiveSessionState.Completed => TimeSpan.FromMinutes(30), // 30 minutes after completion
            _ => TimeSpan.FromMinutes(30)
        };
    }
}

/// <summary>
/// States for live quiz session lifecycle
/// </summary>
public enum LiveSessionState
{
    Lobby,      // Players joining, waiting to start
    Active,     // Game in progress
    Paused,     // Game temporarily paused
    Completed   // Game finished
}