using System.ComponentModel.DataAnnotations;

namespace VibeGuess.Core.Entities;

/// <summary>
/// Represents a question within a quiz.
/// </summary>
public class Question : BaseEntity
{

    /// <summary>
    /// Foreign key to the Quiz this question belongs to.
    /// </summary>
    public Guid QuizId { get; set; }

    /// <summary>
    /// Order of this question within the quiz (0-based index).
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// The question text displayed to the user.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Type of question.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // MultipleChoice, FreeText

    /// <summary>
    /// Whether this question requires audio playback to answer.
    /// </summary>
    public bool RequiresAudio { get; set; } = true;

    /// <summary>
    /// Points awarded for correctly answering this question.
    /// </summary>
    [Range(1, 100)]
    public int Points { get; set; } = 10;

    /// <summary>
    /// Time limit for answering this question (seconds).
    /// </summary>
    [Range(5, 300)]
    public int? TimeLimitSeconds { get; set; }

    /// <summary>
    /// Hint text to help users answer the question.
    /// </summary>
    [MaxLength(200)]
    public string? HintText { get; set; }

    /// <summary>
    /// Explanation provided after answering (educational content).
    /// </summary>
    [MaxLength(1000)]
    public string? Explanation { get; set; }



    // Navigation properties

    /// <summary>
    /// The quiz this question belongs to.
    /// </summary>
    public Quiz Quiz { get; set; } = null!;

    /// <summary>
    /// The Spotify track associated with this question.
    /// </summary>
    public Track? Track { get; set; }

    /// <summary>
    /// Available answer options for this question.
    /// </summary>
    public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();

    /// <summary>
    /// User answers given for this question across different sessions.
    /// </summary>
    public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();

    // Helper properties

    /// <summary>
    /// Gets the correct answer option for multiple choice questions.
    /// </summary>
    public AnswerOption? CorrectAnswer => AnswerOptions.FirstOrDefault(a => a.IsCorrect);

    /// <summary>
    /// Gets the correct answer text for free text questions.
    /// </summary>
    public string? CorrectAnswerText => Type == "FreeText" ? CorrectAnswer?.AnswerText : null;
}