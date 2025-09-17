using System.ComponentModel.DataAnnotations;

namespace VibeGuess.Core.Entities;

/// <summary>
/// Represents an answer option for a multiple choice question.
/// </summary>
public class AnswerOption : BaseEntity
{

    /// <summary>
    /// Foreign key to the Question this option belongs to.
    /// </summary>
    public Guid QuestionId { get; set; }

    /// <summary>
    /// Display order of this option (A, B, C, D).
    /// </summary>
    [Required]
    [MaxLength(5)]
    public string OptionLabel { get; set; } = string.Empty;

    /// <summary>
    /// The answer text displayed to the user.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string AnswerText { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the correct answer.
    /// </summary>
    public bool IsCorrect { get; set; }

    /// <summary>
    /// Order of this option within the question (0-based index).
    /// </summary>
    public int OrderIndex { get; set; }



    // Navigation properties

    /// <summary>
    /// The question this answer option belongs to.
    /// </summary>
    public Question Question { get; set; } = null!;

    /// <summary>
    /// User answers that selected this option.
    /// </summary>
    public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}