using System.ComponentModel.DataAnnotations;

namespace VibeGuess.Core.Entities;

/// <summary>
/// Represents user preference settings for the VibeGuess application.
/// </summary>
public class UserSettings : UserOwnedEntity
{

    /// <summary>
    /// User's preferred language for the interface (ISO 639-1 code).
    /// </summary>
    [Required]
    [MaxLength(5)]
    public string PreferredLanguage { get; set; } = "en";

    /// <summary>
    /// Whether to enable audio preview for questions.
    /// </summary>
    public bool EnableAudioPreview { get; set; } = true;

    /// <summary>
    /// Default number of questions for new quizzes (5-20).
    /// </summary>
    [Range(5, 20)]
    public int DefaultQuestionCount { get; set; } = 10;

    /// <summary>
    /// Default difficulty level for new quizzes.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string DefaultDifficulty { get; set; } = "Medium";

    /// <summary>
    /// Whether to remember the last selected Spotify device.
    /// </summary>
    public bool RememberDeviceSelection { get; set; } = true;

    /// <summary>
    /// Last selected Spotify device ID for playback.
    /// </summary>
    [MaxLength(100)]
    public string? LastSelectedDeviceId { get; set; }

    /// <summary>
    /// Whether to enable hints during quiz sessions.
    /// </summary>
    public bool EnableHints { get; set; } = true;

    /// <summary>
    /// Whether to shuffle questions by default.
    /// </summary>
    public bool ShuffleQuestions { get; set; } = false;



    // Navigation properties

    /// <summary>
    /// The user these settings belong to.
    /// </summary>
    public User User { get; set; } = null!;
}