namespace VibeGuess.Api.Models.Responses;

/// <summary>
/// Response model for hosted session creation
/// </summary>
public class CreateHostedSessionResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string JoinCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Response model for hosted session information
/// </summary>
public class HostedSessionInfoResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string JoinCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int CurrentQuestionIndex { get; set; }
    public int ParticipantCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int QuestionTimeLimit { get; set; }
    public List<ParticipantSummary> Participants { get; set; } = [];
}

/// <summary>
/// Summary information about a session participant
/// </summary>
public class ParticipantSummary
{
    public string ParticipantId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalAnswers { get; set; }
    public bool IsConnected { get; set; }
    public DateTime JoinedAt { get; set; }
    public double Accuracy => TotalAnswers > 0 ? (double)CorrectAnswers / TotalAnswers * 100 : 0;
}

/// <summary>
/// Response model for session analytics summary
/// </summary>
public class SessionSummaryResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public TimeSpan? Duration => StartedAt.HasValue && EndedAt.HasValue ? EndedAt - StartedAt : null;
    
    public SessionStats Stats { get; set; } = new();
    public List<ParticipantSummary> FinalLeaderboard { get; set; } = [];
    public List<QuestionStats> QuestionStats { get; set; } = [];
}

/// <summary>
/// Overall session statistics
/// </summary>
public class SessionStats
{
    public int TotalParticipants { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalAnswers { get; set; }
    public double AverageScore { get; set; }
    public double AverageAccuracy { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
}

/// <summary>
/// Statistics for individual questions
/// </summary>
public class QuestionStats
{
    public int QuestionIndex { get; set; }
    public int TotalAnswers { get; set; }
    public int CorrectAnswers { get; set; }
    public double AccuracyPercentage => TotalAnswers > 0 ? (double)CorrectAnswers / TotalAnswers * 100 : 0;
    public TimeSpan AverageResponseTime { get; set; }
}