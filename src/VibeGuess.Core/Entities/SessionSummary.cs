using VibeGuess.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace VibeGuess.Core.Entities;

/// <summary>
/// Optional database entity for storing session analytics after completion.
/// This is NOT used during live gameplay - only for historical analytics.
/// Live sessions are stored in Redis cache and optionally persisted here when ended.
/// </summary>
public class SessionSummary : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string SessionId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string JoinCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string QuizId { get; set; } = string.Empty;
    
    public new DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    
    // Session Statistics
    public int ParticipantCount { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalAnswers { get; set; }
    public double AverageScore { get; set; }
    public double AverageAccuracy { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    
    // JSON-serialized data for detailed analytics
    [MaxLength(int.MaxValue)] // Use max for JSON data
    public string? LeaderboardJson { get; set; } // Top 20 participants
    
    [MaxLength(int.MaxValue)] // Use max for JSON data
    public string? QuestionStatsJson { get; set; } // Per-question statistics
    
    [MaxLength(int.MaxValue)] // Use max for JSON data  
    public string? ParticipantDetailsJson { get; set; } // Detailed participant data
    
    // Computed properties
    public TimeSpan? Duration => StartedAt.HasValue && EndedAt.HasValue ? EndedAt - StartedAt : null;
}