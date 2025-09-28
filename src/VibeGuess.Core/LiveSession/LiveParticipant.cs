namespace VibeGuess.Core.LiveSession;

/// <summary>
/// In-memory participant in a live quiz session.
/// Stored in Redis cache as part of session state.
/// </summary>
public class LiveParticipant
{
    public required string ParticipantId { get; set; }
    public required string ConnectionId { get; set; }
    public required string DisplayName { get; set; }
    
    public int Score { get; set; } = 0;
    public int CorrectAnswers { get; set; } = 0;
    public int TotalAnswers { get; set; } = 0;
    
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public bool IsConnected { get; set; } = true;
    
    // Current question state
    public bool HasAnsweredCurrentQuestion { get; set; } = false;
    public DateTime? CurrentQuestionAnsweredAt { get; set; }
    
    public double GetAccuracy()
    {
        return TotalAnswers > 0 ? (double)CorrectAnswers / TotalAnswers * 100 : 0;
    }
    
    public void UpdateActivity()
    {
        LastActivity = DateTime.UtcNow;
    }
}