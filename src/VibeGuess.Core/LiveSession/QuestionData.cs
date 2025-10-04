using System.ComponentModel.DataAnnotations;

namespace VibeGuess.Core.LiveSession;

/// <summary>
/// Represents question data for a live quiz session
/// </summary>
public class QuestionData
{
    /// <summary>
    /// Unique identifier for the question
    /// </summary>
    [Required]
    public required string QuestionId { get; set; }
    
    /// <summary>
    /// The question text displayed to participants
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 5)]
    public required string QuestionText { get; set; }
    
    /// <summary>
    /// Available answer options
    /// </summary>
    [Required]
    [MinLength(2, ErrorMessage = "At least 2 options are required")]
    [MaxLength(6, ErrorMessage = "Maximum 6 options allowed")]
    public required List<string> Options { get; set; }
    
    /// <summary>
    /// The correct answer (should match one of the Options exactly)
    /// </summary>
    [Required]
    public required string CorrectAnswer { get; set; }
    
    /// <summary>
    /// Question type
    /// </summary>
    public QuestionType Type { get; set; } = QuestionType.MultipleChoice;
    
    /// <summary>
    /// Time limit for this question in seconds (overrides session default if specified)
    /// </summary>
    [Range(5, 300, ErrorMessage = "Time limit must be between 5 and 300 seconds")]
    public int? TimeLimit { get; set; }
    
    /// <summary>
    /// Points awarded for correct answer (overrides session default if specified)
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Points must be between 1 and 1000")]
    public int? Points { get; set; }
    
    /// <summary>
    /// Additional metadata for the question
    /// </summary>
    public QuestionMetadata? Metadata { get; set; }
    
    /// <summary>
    /// Validates that the correct answer is one of the provided options
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(CorrectAnswer) || Options == null || !Options.Any())
            return false;
            
        return Options.Contains(CorrectAnswer, StringComparer.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Gets the question for participants (without the correct answer)
    /// </summary>
    public ParticipantQuestionData ToParticipantView()
    {
        return new ParticipantQuestionData
        {
            QuestionId = QuestionId,
            QuestionText = QuestionText,
            Options = Options.ToList(), // Create a copy
            Type = Type,
            TimeLimit = TimeLimit,
            Metadata = Metadata
        };
    }
}

/// <summary>
/// Question data sent to participants (without correct answer)
/// </summary>
public class ParticipantQuestionData
{
    public required string QuestionId { get; set; }
    public required string QuestionText { get; set; }
    public required List<string> Options { get; set; }
    public QuestionType Type { get; set; }
    public int? TimeLimit { get; set; }
    public QuestionMetadata? Metadata { get; set; }
}

/// <summary>
/// Types of questions supported
/// </summary>
public enum QuestionType
{
    MultipleChoice,
    TrueFalse,
    Text
}

/// <summary>
/// Additional metadata for questions
/// </summary>
public class QuestionMetadata
{
    /// <summary>
    /// Difficulty level (1-5 scale)
    /// </summary>
    [Range(1, 5)]
    public int? Difficulty { get; set; }
    
    /// <summary>
    /// Question category
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// Tags for the question
    /// </summary>
    public List<string>? Tags { get; set; }
    
    /// <summary>
    /// Explanation shown after answering (optional)
    /// </summary>
    public string? Explanation { get; set; }
    
    /// <summary>
    /// Source or attribution for the question
    /// </summary>
    public string? Source { get; set; }
}