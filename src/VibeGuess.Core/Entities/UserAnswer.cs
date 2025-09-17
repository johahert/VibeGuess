using System.ComponentModel.DataAnnotations;

namespace VibeGuess.Core.Entities;

/// <summary>
/// Represents a user's answer to a question during a quiz session.
/// </summary>
public class UserAnswer : BaseEntity
{

    /// <summary>
    /// Foreign key to the QuizSession this answer belongs to.
    /// </summary>
    public Guid QuizSessionId { get; set; }

    /// <summary>
    /// Foreign key to the Question being answered.
    /// </summary>
    public Guid QuestionId { get; set; }

    /// <summary>
    /// Foreign key to the selected AnswerOption (for multiple choice questions).
    /// </summary>
    public Guid? SelectedAnswerOptionId { get; set; }

    /// <summary>
    /// Free text answer (for free text questions).
    /// </summary>
    [MaxLength(500)]
    public string? FreeTextAnswer { get; set; }

    /// <summary>
    /// Whether the answer is correct.
    /// </summary>
    public bool IsCorrect { get; set; }

    /// <summary>
    /// Points earned for this answer.
    /// </summary>
    public int PointsEarned { get; set; } = 0;

    /// <summary>
    /// Time taken to answer this question (seconds).
    /// </summary>
    public int? TimeToAnswerSeconds { get; set; }

    /// <summary>
    /// Whether the user used a hint for this question.
    /// </summary>
    public bool UsedHint { get; set; } = false;

    /// <summary>
    /// Number of times the user listened to the track preview.
    /// </summary>
    public int PlaybackCount { get; set; } = 0;

    /// <summary>
    /// When the answer was submitted.
    /// </summary>
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the question was first presented to the user.
    /// </summary>
    public DateTime QuestionStartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional data about the answer in JSON format.
    /// </summary>
    [MaxLength(1000)]
    public string? AnswerMetadata { get; set; }

    // Navigation properties

    /// <summary>
    /// The quiz session this answer belongs to.
    /// </summary>
    public QuizSession QuizSession { get; set; } = null!;

    /// <summary>
    /// The question being answered.
    /// </summary>
    public Question Question { get; set; } = null!;

    /// <summary>
    /// The selected answer option (for multiple choice questions).
    /// </summary>
    public AnswerOption? SelectedAnswerOption { get; set; }

    // Helper properties

    /// <summary>
    /// Time taken to answer in a human-readable format.
    /// </summary>
    public string FormattedTimeToAnswer
    {
        get
        {
            if (!TimeToAnswerSeconds.HasValue) return "N/A";
            var timeSpan = TimeSpan.FromSeconds(TimeToAnswerSeconds.Value);
            return timeSpan.TotalMinutes >= 1 
                ? $"{timeSpan.Minutes}m {timeSpan.Seconds}s"
                : $"{timeSpan.Seconds}s";
        }
    }

    /// <summary>
    /// Gets the answer text (either from selected option or free text).
    /// </summary>
    public string AnswerText => SelectedAnswerOption?.AnswerText ?? FreeTextAnswer ?? "No answer";

    /// <summary>
    /// Whether this is a multiple choice answer.
    /// </summary>
    public bool IsMultipleChoice => SelectedAnswerOptionId.HasValue;

    /// <summary>
    /// Whether this is a free text answer.
    /// </summary>
    public bool IsFreeText => !string.IsNullOrEmpty(FreeTextAnswer);
}