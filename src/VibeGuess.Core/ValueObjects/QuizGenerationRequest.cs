using System.ComponentModel.DataAnnotations;

namespace VibeGuess.Core.ValueObjects;

/// <summary>
/// Value object representing a quiz generation request.
/// </summary>
public record QuizGenerationRequest
{
    /// <summary>
    /// User prompt for generating the quiz.
    /// </summary>
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Prompt { get; init; } = string.Empty;

    /// <summary>
    /// Number of questions to generate.
    /// </summary>
    [Range(5, 20)]
    public int QuestionCount { get; init; } = 10;

    /// <summary>
    /// Format of the quiz.
    /// </summary>
    [Required]
    public string Format { get; init; } = string.Empty;

    /// <summary>
    /// Difficulty level of the quiz.
    /// </summary>
    public string Difficulty { get; init; } = "Medium";

    /// <summary>
    /// Whether to include audio playback in questions.
    /// </summary>
    public bool IncludeAudio { get; init; } = true;

    /// <summary>
    /// Language for the quiz content.
    /// </summary>
    [StringLength(5, MinimumLength = 2)]
    public string Language { get; init; } = "en";

    /// <summary>
    /// Validates the request parameters.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Prompt))
            errors.Add("Prompt is required.");
        else if (Prompt.Length < 10 || Prompt.Length > 1000)
            errors.Add("Prompt must be between 10 and 1000 characters.");

        if (QuestionCount < 5 || QuestionCount > 20)
            errors.Add("Question count must be between 5 and 20.");

        if (!Entities.QuizFormat.IsValid(Format))
            errors.Add($"Format must be one of: {string.Join(", ", Entities.QuizFormat.All)}.");

        if (!string.IsNullOrEmpty(Difficulty) && !Entities.QuizDifficulty.IsValid(Difficulty))
            errors.Add($"Difficulty must be one of: {string.Join(", ", Entities.QuizDifficulty.All)}.");

        if (string.IsNullOrWhiteSpace(Language))
            errors.Add("Language is required.");
        else if (Language.Length != 2 && Language.Length != 5)
            errors.Add("Language must be a valid ISO 639-1 code (e.g., 'en', 'en-US').");

        return errors.Count == 0;
    }
}