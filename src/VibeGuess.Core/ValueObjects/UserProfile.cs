namespace VibeGuess.Core.ValueObjects;

/// <summary>
/// Value object representing user profile information.
/// </summary>
public record UserProfile
{
    /// <summary>
    /// User identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// User's display name.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Whether the user has Spotify Premium.
    /// </summary>
    public bool HasSpotifyPremium { get; init; }

    /// <summary>
    /// User's country code.
    /// </summary>
    public string Country { get; init; } = string.Empty;

    /// <summary>
    /// URL to user's profile image.
    /// </summary>
    public string? ProfileImageUrl { get; init; }

    /// <summary>
    /// When the user was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the user last logged in.
    /// </summary>
    public DateTime LastLoginAt { get; init; }

    /// <summary>
    /// User's role in the system.
    /// </summary>
    public string Role { get; init; } = string.Empty;
}

/// <summary>
/// Value object representing user settings.
/// </summary>
public record UserSettingsProfile
{
    /// <summary>
    /// Preferred language for the interface.
    /// </summary>
    public string PreferredLanguage { get; init; } = string.Empty;

    /// <summary>
    /// Whether to enable audio preview.
    /// </summary>
    public bool EnableAudioPreview { get; init; }

    /// <summary>
    /// Default number of questions for new quizzes.
    /// </summary>
    public int DefaultQuestionCount { get; init; }

    /// <summary>
    /// Default difficulty for new quizzes.
    /// </summary>
    public string DefaultDifficulty { get; init; } = string.Empty;

    /// <summary>
    /// Whether to remember device selection.
    /// </summary>
    public bool RememberDeviceSelection { get; init; }

    /// <summary>
    /// Whether to enable hints by default.
    /// </summary>
    public bool EnableHints { get; init; }

    /// <summary>
    /// Whether to shuffle questions by default.
    /// </summary>
    public bool ShuffleQuestions { get; init; }
}

/// <summary>
/// Value object representing the complete user profile response.
/// </summary>
public record UserProfileResponse
{
    /// <summary>
    /// User profile information.
    /// </summary>
    public UserProfile User { get; init; } = new();

    /// <summary>
    /// User settings.
    /// </summary>
    public UserSettingsProfile Settings { get; init; } = new();
}