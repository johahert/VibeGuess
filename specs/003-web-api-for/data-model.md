# Data Model & Entity Design

**Feature**: VibeGuess Music Quiz API  
**Date**: 2025-09-15  
**Status**: Design Complete

## Core Entities

### User Entity
```csharp
public class User
{
    public Guid Id { get; set; }
    public string SpotifyUserId { get; set; } // External Spotify user ID
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string Country { get; set; } // For regional content filtering
    public bool HasSpotifyPremium { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    
    // Navigation properties
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    public ICollection<QuizSession> QuizSessions { get; set; } = new List<QuizSession>();
    public UserSettings Settings { get; set; }
}
```

**Validation Rules**:
- SpotifyUserId: Required, unique, max 50 characters
- Email: Required, valid email format, max 255 characters
- DisplayName: Required, max 100 characters
- Country: ISO 3166-1 alpha-2 code (2 characters)

### Quiz Entity
```csharp
public class Quiz
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; }
    public string UserPrompt { get; set; } // Original user prompt
    public QuizFormat Format { get; set; }
    public QuizDifficulty Difficulty { get; set; }
    public int QuestionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; } // 24-hour retention
    public QuizStatus Status { get; set; }
    
    // Navigation properties
    public User User { get; set; }
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<QuizSession> Sessions { get; set; } = new List<QuizSession>();
}

public enum QuizFormat
{
    MultipleChoice,
    FreeText,
    Mixed // Future extension point
}

public enum QuizDifficulty
{
    Easy,
    Medium,
    Hard
}

public enum QuizStatus
{
    Generated,
    InProgress,
    Completed,
    Expired
}
```

**Validation Rules**:
- Title: Required, max 200 characters
- UserPrompt: Required, max 1000 characters
- QuestionCount: Range 1-20
- ExpiresAt: Must be within 24 hours of CreatedAt

### Question Entity
```csharp
public class Question
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public int OrderIndex { get; set; }
    public string QuestionText { get; set; }
    public QuestionType Type { get; set; }
    public string CorrectAnswer { get; set; }
    public string? SpotifyTrackId { get; set; } // Optional for audio context
    public bool RequiresAudio { get; set; }
    public string? Hint { get; set; }
    public int Points { get; set; } = 1;
    
    // Navigation properties
    public Quiz Quiz { get; set; }
    public TrackMetadata? Track { get; set; }
    public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
    public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}

public enum QuestionType
{
    MultipleChoice,
    FreeText,
    TrueFalse // Future extension
}
```

**Validation Rules**:
- QuestionText: Required, max 500 characters
- CorrectAnswer: Required, max 200 characters
- OrderIndex: Unique within quiz, non-negative
- SpotifyTrackId: Valid Spotify track ID format when present

### AnswerOption Entity
```csharp
public class AnswerOption
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string OptionText { get; set; }
    public int OrderIndex { get; set; }
    public bool IsCorrect { get; set; }
    
    // Navigation properties
    public Question Question { get; set; }
}
```

**Validation Rules**:
- OptionText: Required, max 200 characters
- OrderIndex: Unique within question, non-negative

### TrackMetadata Entity
```csharp
public class TrackMetadata
{
    public string SpotifyTrackId { get; set; } // Primary key
    public string Name { get; set; }
    public string ArtistName { get; set; }
    public string AlbumName { get; set; }
    public int DurationMs { get; set; }
    public string? PreviewUrl { get; set; }
    public bool IsPlayable { get; set; }
    public string[] AvailableMarkets { get; set; } = Array.Empty<string>();
    public DateTime LastValidated { get; set; }
    
    // Navigation properties
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
```

**Validation Rules**:
- SpotifyTrackId: Required, valid Spotify ID format
- Name: Required, max 200 characters
- ArtistName: Required, max 200 characters
- AlbumName: Required, max 200 characters
- DurationMs: Positive integer

### QuizSession Entity
```csharp
public class QuizSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid QuizId { get; set; }
    public string? SelectedDeviceId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public SessionStatus Status { get; set; }
    public int TotalScore { get; set; }
    public int MaxPossibleScore { get; set; }
    
    // Navigation properties
    public User User { get; set; }
    public Quiz Quiz { get; set; }
    public DeviceInfo? SelectedDevice { get; set; }
    public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    public ICollection<PlaybackEvent> PlaybackHistory { get; set; } = new List<PlaybackEvent>();
}

public enum SessionStatus
{
    InProgress,
    Completed,
    Abandoned,
    Paused
}
```

**Validation Rules**:
- CurrentQuestionIndex: Non-negative, within question count
- TotalScore: Non-negative, ≤ MaxPossibleScore
- MaxPossibleScore: Positive integer

### UserAnswer Entity
```csharp
public class UserAnswer
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid QuestionId { get; set; }
    public string AnswerText { get; set; }
    public Guid? SelectedOptionId { get; set; } // For multiple choice
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
    public DateTime AnsweredAt { get; set; }
    public TimeSpan TimeSpent { get; set; }
    
    // Navigation properties
    public QuizSession Session { get; set; }
    public Question Question { get; set; }
    public AnswerOption? SelectedOption { get; set; }
}
```

**Validation Rules**:
- AnswerText: Required, max 500 characters
- PointsEarned: Non-negative
- TimeSpent: Positive duration

### DeviceInfo Entity
```csharp
public class DeviceInfo
{
    public string SpotifyDeviceId { get; set; } // Primary key
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; } // Computer, Smartphone, Speaker, etc.
    public bool IsActive { get; set; }
    public bool IsRestricted { get; set; }
    public int? VolumePercent { get; set; }
    public DateTime LastSeen { get; set; }
    
    // Navigation properties
    public User User { get; set; }
    public ICollection<QuizSession> Sessions { get; set; } = new List<QuizSession>();
    public ICollection<PlaybackEvent> PlaybackEvents { get; set; } = new List<PlaybackEvent>();
}
```

**Validation Rules**:
- SpotifyDeviceId: Required, valid Spotify device ID
- Name: Required, max 100 characters
- Type: Required, max 50 characters
- VolumePercent: Range 0-100 when present

### PlaybackEvent Entity
```csharp
public class PlaybackEvent
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string DeviceId { get; set; }
    public string? TrackId { get; set; }
    public PlaybackAction Action { get; set; }
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int? Position { get; set; } // Track position in ms
    
    // Navigation properties
    public QuizSession Session { get; set; }
    public DeviceInfo Device { get; set; }
}

public enum PlaybackAction
{
    Play,
    Pause,
    Resume,
    Seek,
    VolumeChange,
    DeviceTransfer
}
```

**Validation Rules**:
- Action: Required enum value
- Position: Non-negative when present
- ErrorMessage: Max 500 characters when present

### UserSettings Entity
```csharp
public class UserSettings
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public bool EnableAudioPreview { get; set; } = true;
    public int DefaultQuestionCount { get; set; } = 10;
    public QuizDifficulty DefaultDifficulty { get; set; } = QuizDifficulty.Medium;
    public bool RememberDeviceSelection { get; set; } = false;
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; }
}
```

**Validation Rules**:
- PreferredLanguage: ISO 639-1 language code
- DefaultQuestionCount: Range 5-20

## Entity Relationships

### Primary Relationships
- **User** → **Quiz** (One-to-Many): A user can create multiple quizzes
- **Quiz** → **Question** (One-to-Many): A quiz contains multiple questions
- **Question** → **AnswerOption** (One-to-Many): Multiple choice questions have options
- **Question** → **TrackMetadata** (Many-to-One): Multiple questions can reference same track
- **User** → **QuizSession** (One-to-Many): User can have multiple active sessions
- **Quiz** → **QuizSession** (One-to-Many): Quiz can be taken multiple times
- **QuizSession** → **UserAnswer** (One-to-Many): Session contains user's answers
- **User** → **DeviceInfo** (One-to-Many): User has multiple Spotify devices
- **QuizSession** → **PlaybackEvent** (One-to-Many): Session tracks playback history

### Indexes & Performance
```sql
-- Primary performance indexes
CREATE INDEX IX_Quiz_UserId_CreatedAt ON Quiz (UserId, CreatedAt DESC);
CREATE INDEX IX_Quiz_ExpiresAt ON Quiz (ExpiresAt) WHERE Status != 'Expired';
CREATE INDEX IX_Question_QuizId_OrderIndex ON Question (QuizId, OrderIndex);
CREATE INDEX IX_QuizSession_UserId_Status ON QuizSession (UserId, Status);
CREATE INDEX IX_TrackMetadata_LastValidated ON TrackMetadata (LastValidated);
CREATE INDEX IX_PlaybackEvent_SessionId_Timestamp ON PlaybackEvent (SessionId, Timestamp);

-- Unique constraints
ALTER TABLE User ADD CONSTRAINT UK_User_SpotifyUserId UNIQUE (SpotifyUserId);
ALTER TABLE Question ADD CONSTRAINT UK_Question_Quiz_OrderIndex UNIQUE (QuizId, OrderIndex);
ALTER TABLE AnswerOption ADD CONSTRAINT UK_AnswerOption_Question_OrderIndex UNIQUE (QuestionId, OrderIndex);
```

## Data Validation & Business Rules

### Quiz Generation Rules
1. **Question Count**: 5-20 questions per quiz based on user prompt complexity
2. **Track Selection**: Maximum 80% of questions can have audio components
3. **Content Filtering**: All AI-generated content must pass appropriateness validation
4. **Regional Compliance**: Track availability verified against user's country
5. **Retention Policy**: Quizzes automatically expire after 24 hours

### Session Management Rules
1. **Concurrent Sessions**: User can have maximum 3 active quiz sessions
2. **Device Selection**: Required for any playback-enabled question
3. **Progress Tracking**: Auto-save progress every 30 seconds
4. **Session Timeout**: Abandon sessions inactive for 2 hours

### Playback Control Rules
1. **Premium Validation**: Full track playback requires Spotify Premium
2. **Rate Limiting**: Maximum 60 playback commands per minute per user
3. **Device Availability**: Validate device is active before playback commands
4. **Error Recovery**: Retry failed playback commands with exponential backoff

## Database Configuration

### Connection String Template
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=VibeGuessDb;Trusted_Connection=true;TrustServerCertificate=true;",
    "TestConnection": "Server=localhost;Database=VibeGuessDb_Test;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### Entity Framework Configuration
```csharp
public class VibeGuessDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<AnswerOption> AnswerOptions { get; set; }
    public DbSet<TrackMetadata> TrackMetadata { get; set; }
    public DbSet<QuizSession> QuizSessions { get; set; }
    public DbSet<UserAnswer> UserAnswers { get; set; }
    public DbSet<DeviceInfo> DeviceInfos { get; set; }
    public DbSet<PlaybackEvent> PlaybackEvents { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entity relationships, indexes, and constraints
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VibeGuessDbContext).Assembly);
        
        // Global query filters
        modelBuilder.Entity<Quiz>()
            .HasQueryFilter(q => q.ExpiresAt > DateTime.UtcNow);
    }
}
```

---
*Data model design complete. All entities support constitutional requirements for testing, performance, and extensibility.*