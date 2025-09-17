using System.ComponentModel.DataAnnotations;

namespace VibeGuess.Core.Entities;

/// <summary>
/// Represents the difficulty levels available in the system.
/// </summary>
public static class QuizDifficulty
{
    public const string Easy = "Easy";
    public const string Medium = "Medium";
    public const string Hard = "Hard";

    public static readonly string[] All = { Easy, Medium, Hard };

    public static bool IsValid(string difficulty)
    {
        return All.Contains(difficulty, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Represents the quiz format types available in the system.
/// </summary>
public static class QuizFormat
{
    public const string MultipleChoice = "MultipleChoice";
    public const string FreeText = "FreeText";
    public const string Mixed = "Mixed";

    public static readonly string[] All = { MultipleChoice, FreeText, Mixed };

    public static bool IsValid(string format)
    {
        return All.Contains(format, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Represents the question types available in the system.
/// </summary>
public static class QuestionType
{
    public const string MultipleChoice = "MultipleChoice";
    public const string FreeText = "FreeText";

    public static readonly string[] All = { MultipleChoice, FreeText };

    public static bool IsValid(string type)
    {
        return All.Contains(type, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Represents the quiz status values.
/// </summary>
public static class QuizStatus
{
    public const string Generated = "Generated";
    public const string Archived = "Archived";
    public const string Deleted = "Deleted";

    public static readonly string[] All = { Generated, Archived, Deleted };

    public static bool IsValid(string status)
    {
        return All.Contains(status, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Represents the quiz session status values.
/// </summary>
public static class QuizSessionStatus
{
    public const string Active = "Active";
    public const string Completed = "Completed";
    public const string Paused = "Paused";
    public const string Abandoned = "Abandoned";

    public static readonly string[] All = { Active, Completed, Paused, Abandoned };

    public static bool IsValid(string status)
    {
        return All.Contains(status, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Represents user roles in the system.
/// </summary>
public static class UserRole
{
    public const string User = "User";
    public const string Admin = "Admin";

    public static readonly string[] All = { User, Admin };

    public static bool IsValid(string role)
    {
        return All.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}