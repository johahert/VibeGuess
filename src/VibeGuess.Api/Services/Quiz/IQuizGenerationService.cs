using VibeGuess.Core.Entities;

namespace VibeGuess.Api.Services.Quiz;

/// <summary>
/// Service for generating music quizzes using AI and Spotify integration.
/// </summary>
public interface IQuizGenerationService
{
    /// <summary>
    /// Generates a new quiz based on the provided request.
    /// </summary>
    /// <param name="request">Quiz generation request</param>
    /// <param name="userId">ID of the user creating the quiz</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The generated quiz with questions and metadata</returns>
    Task<QuizGenerationResult> GenerateQuizAsync(QuizGenerationRequest request, Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request model for quiz generation.
/// </summary>
public class QuizGenerationRequest
{
    public string Prompt { get; set; } = string.Empty;
    public int QuestionCount { get; set; } = 10;
    public string Format { get; set; } = "MultipleChoice";
    public string Difficulty { get; set; } = "Medium";
    public bool IncludeAudio { get; set; } = true;
    public string Language { get; set; } = "en";
}

/// <summary>
/// Result of quiz generation process.
/// </summary>
public class QuizGenerationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public VibeGuess.Core.Entities.Quiz? Quiz { get; set; }
    public QuizGenerationMetadata? Metadata { get; set; }
}

/// <summary>
/// Metadata about the quiz generation process.
/// </summary>
public class QuizGenerationMetadata
{
    public double ProcessingTimeMs { get; set; }
    public string AiModel { get; set; } = string.Empty;
    public int TracksFound { get; set; }
    public int TracksValidated { get; set; }
    public int TracksFailed { get; set; }
    public List<string> GenerationSteps { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}