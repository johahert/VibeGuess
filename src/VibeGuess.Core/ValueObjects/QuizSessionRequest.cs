using System.ComponentModel.DataAnnotations;

namespace VibeGuess.Core.ValueObjects;

/// <summary>
/// Value object representing a request to start a quiz session.
/// </summary>
public record StartQuizSessionRequest
{
    /// <summary>
    /// Spotify device ID where audio will be played.
    /// </summary>
    [Required]
    public string DeviceId { get; init; } = string.Empty;

    /// <summary>
    /// Whether to shuffle the order of questions.
    /// </summary>
    public bool ShuffleQuestions { get; init; } = false;

    /// <summary>
    /// Whether to enable hints during the quiz.
    /// </summary>
    public bool EnableHints { get; init; } = true;

    /// <summary>
    /// Session timeout in minutes (optional).
    /// </summary>
    [Range(5, 480)] // 5 minutes to 8 hours
    public int? TimeoutMinutes { get; init; }

    /// <summary>
    /// Validates the request parameters.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(DeviceId))
            errors.Add("DeviceId is required.");

        if (TimeoutMinutes.HasValue && (TimeoutMinutes < 5 || TimeoutMinutes > 480))
            errors.Add("Timeout must be between 5 and 480 minutes.");

        return errors.Count == 0;
    }
}

/// <summary>
/// Value object representing a quiz session response.
/// </summary>
public record QuizSessionResponse
{
    /// <summary>
    /// Unique session identifier.
    /// </summary>
    public Guid SessionId { get; init; }

    /// <summary>
    /// Quiz identifier being played.
    /// </summary>
    public Guid QuizId { get; init; }

    /// <summary>
    /// When the session was started.
    /// </summary>
    public DateTime StartedAt { get; init; }

    /// <summary>
    /// Current question index (0-based).
    /// </summary>
    public int CurrentQuestionIndex { get; init; }

    /// <summary>
    /// Total number of questions in the quiz.
    /// </summary>
    public int TotalQuestions { get; init; }

    /// <summary>
    /// Current session status.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Current score.
    /// </summary>
    public int CurrentScore { get; init; }

    /// <summary>
    /// Maximum possible score.
    /// </summary>
    public int MaxPossibleScore { get; init; }

    /// <summary>
    /// When the session expires.
    /// </summary>
    public DateTime ExpiresAt { get; init; }
}