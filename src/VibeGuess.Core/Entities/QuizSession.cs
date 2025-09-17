using System.ComponentModel.DataAnnotations;
using VibeGuess.Core.Interfaces;

namespace VibeGuess.Core.Entities;

/// <summary>
/// Represents an active quiz session for a user.
/// </summary>
public class QuizSession : ExpirableEntity, IUserOwned
{

    /// <summary>
    /// Foreign key to the Quiz being played.
    /// </summary>
    public Guid QuizId { get; set; }

    /// <summary>
    /// Foreign key to the User playing the quiz.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Spotify device ID where audio is being played.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Current question index (0-based).
    /// </summary>
    public int CurrentQuestionIndex { get; set; } = 0;

    /// <summary>
    /// Total number of questions in this session.
    /// </summary>
    public int TotalQuestions { get; set; }

    /// <summary>
    /// Current status of the session.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Active"; // Active, Completed, Paused, Abandoned

    /// <summary>
    /// Current score (points earned so far).
    /// </summary>
    public int CurrentScore { get; set; } = 0;

    /// <summary>
    /// Maximum possible score for this session.
    /// </summary>
    public int MaxPossibleScore { get; set; }

    /// <summary>
    /// Whether questions are shuffled in this session.
    /// </summary>
    public bool ShuffleQuestions { get; set; } = false;

    /// <summary>
    /// Whether hints are enabled for this session.
    /// </summary>
    public bool EnableHints { get; set; } = true;

    /// <summary>
    /// When the session was started.
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the session was completed (if applicable).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Total time spent on the session (seconds).
    /// </summary>
    public int? TotalTimeSeconds { get; set; }

    /// <summary>
    /// Session configuration in JSON format.
    /// </summary>
    [MaxLength(2000)]
    public string? SessionConfig { get; set; }

    // Navigation properties

    /// <summary>
    /// The quiz being played in this session.
    /// </summary>
    public Quiz Quiz { get; set; } = null!;

    /// <summary>
    /// The user playing this session.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// User answers given during this session.
    /// </summary>
    public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();

    // Helper properties

    /// <summary>
    /// Whether the session is currently active.
    /// </summary>
    public bool IsActive => Status == "Active" && !IsExpired;

    /// <summary>
    /// Whether the session has expired and is still active (needs to be marked as expired).
    /// </summary>
    public bool IsExpiredButActive => DateTime.UtcNow >= ExpiresAt && Status == "Active";

    /// <summary>
    /// Current score as a percentage.
    /// </summary>
    public decimal ScorePercentage => MaxPossibleScore > 0 ? (decimal)CurrentScore / MaxPossibleScore * 100 : 0;

    /// <summary>
    /// Number of questions answered so far.
    /// </summary>
    public int QuestionsAnswered => UserAnswers.Count;

    /// <summary>
    /// Number of correct answers given.
    /// </summary>
    public int CorrectAnswers => UserAnswers.Count(a => a.IsCorrect);

    /// <summary>
    /// Whether the session is completed.
    /// </summary>
    public bool IsCompleted => Status == "Completed" || CurrentQuestionIndex >= TotalQuestions;

    /// <summary>
    /// Session duration so far.
    /// </summary>
    public TimeSpan Duration => CompletedAt?.Subtract(StartedAt) ?? DateTime.UtcNow.Subtract(StartedAt);

    /// <summary>
    /// Override to set default expiration of 2 hours from creation.
    /// </summary>
    public override void UpdateTimestamp()
    {
        base.UpdateTimestamp();
        if (ExpiresAt == default)
        {
            ExpiresAt = DateTime.UtcNow.AddHours(2);
        }
    }
}