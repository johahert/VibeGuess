namespace VibeGuess.Core.LiveSession;

/// <summary>
/// In-memory answer submitted during live quiz gameplay.
/// Stored in Redis cache for real-time scoring and leaderboard calculations.
/// </summary>
public class LiveAnswer
{
    public required string AnswerId { get; set; }
    public required string SessionId { get; set; }
    public required string ParticipantId { get; set; }
    public required int QuestionIndex { get; set; }
    
    public required string SelectedAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public int BaseScore { get; set; } = 100; // Base points for correct answer
    public int TimeBonus { get; set; } = 0;   // Bonus points based on speed
    public int TotalScore { get; set; } = 0;  // BaseScore + TimeBonus (if correct)
    
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ResponseTime { get; set; } // Time from question start to answer
    
    /// <summary>
    /// Calculate time bonus based on response speed
    /// Faster responses get more bonus points (0-50% of base score)
    /// </summary>
    public void CalculateTimeBonus(int questionTimeLimitSeconds, int? customBaseScore = null)
    {
        if (customBaseScore.HasValue)
        {
            BaseScore = customBaseScore.Value;
        }
        
        if (!IsCorrect)
        {
            TimeBonus = 0;
            TotalScore = 0;
            return;
        }
        
        var responseSeconds = ResponseTime.TotalSeconds;
        var remainingTimePercentage = Math.Max(0, (questionTimeLimitSeconds - responseSeconds) / questionTimeLimitSeconds);
        
        // Max 50% of base score as bonus for fastest responses
        TimeBonus = (int)(BaseScore * 0.5 * remainingTimePercentage);
        TotalScore = BaseScore + TimeBonus;
    }
}