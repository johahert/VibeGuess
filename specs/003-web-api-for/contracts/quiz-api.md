# API Quiz Generation Endpoints

**Base URL**: `/api/quiz`  
**Version**: v1  
**Authentication**: Bearer Token (required)

## Endpoints Overview

### POST /api/quiz/generate
**Purpose**: Generate a new music quiz based on user prompt  
**Authentication**: Bearer Token  

#### Request
```json
{
  "prompt": "Create a quiz about 80s rock bands and their hit songs",
  "questionCount": 10,
  "format": "MultipleChoice",
  "difficulty": "Medium",
  "includeAudio": true,
  "language": "en"
}
```

#### Request Validation
- `prompt`: Required, 10-1000 characters, non-empty after trimming
- `questionCount`: Optional, range 5-20, default 10
- `format`: Optional, enum ["MultipleChoice", "FreeText", "Mixed"], default "MultipleChoice"
- `difficulty`: Optional, enum ["Easy", "Medium", "Hard"], default "Medium"
- `includeAudio`: Optional, boolean, default true
- `language`: Optional, ISO 639-1 code, default "en"

#### Response (200 OK)
```json
{
  "quiz": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "title": "80s Rock Bands Quiz",
    "userPrompt": "Create a quiz about 80s rock bands and their hit songs",
    "format": "MultipleChoice",
    "difficulty": "Medium",
    "questionCount": 10,
    "createdAt": "2025-09-15T12:00:00Z",
    "expiresAt": "2025-09-16T12:00:00Z",
    "status": "Generated",
    "questions": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440001",
        "orderIndex": 1,
        "questionText": "Which band released the hit song 'Don't Stop Believin'' in 1981?",
        "type": "MultipleChoice",
        "requiresAudio": true,
        "points": 1,
        "hint": "This band was formed in San Francisco",
        "track": {
          "spotifyTrackId": "4VqPOruhp5EdPBeR92t6lQ",
          "name": "Don't Stop Believin'",
          "artistName": "Journey",
          "albumName": "Escape",
          "durationMs": 251000,
          "previewUrl": "https://p.scdn.co/mp3-preview/...",
          "isPlayable": true
        },
        "answerOptions": [
          {
            "id": "550e8400-e29b-41d4-a716-446655440002",
            "orderIndex": 1,
            "optionText": "Journey",
            "isCorrect": true
          },
          {
            "id": "550e8400-e29b-41d4-a716-446655440003",
            "orderIndex": 2,
            "optionText": "Foreigner",
            "isCorrect": false
          },
          {
            "id": "550e8400-e29b-41d4-a716-446655440004",
            "orderIndex": 3,
            "optionText": "REO Speedwagon",
            "isCorrect": false
          },
          {
            "id": "550e8400-e29b-41d4-a716-446655440005",
            "orderIndex": 4,
            "optionText": "Boston",
            "isCorrect": false
          }
        ]
      }
    ]
  },
  "generationMetadata": {
    "processingTimeMs": 4250,
    "aiModel": "gpt-4",
    "tracksFound": 8,
    "tracksValidated": 8,
    "tracksFailed": 0
  }
}
```

#### Response (400 Bad Request)
```json
{
  "error": "invalid_request",
  "message": "Prompt is required and must be between 10-1000 characters",
  "correlationId": "abc-123-def",
  "details": {
    "prompt": "Field is required"
  }
}
```

#### Response (422 Unprocessable Entity)
```json
{
  "error": "content_generation_failed",
  "message": "Unable to generate sufficient quiz content for the provided prompt",
  "correlationId": "abc-123-def",
  "details": {
    "reason": "Insufficient track matches found",
    "suggestedPrompts": [
      "Try a broader music topic",
      "Include specific artists or genres",
      "Specify a time period or decade"
    ]
  }
}
```

---

### GET /api/quiz/{quizId}
**Purpose**: Retrieve a specific quiz by ID  
**Authentication**: Bearer Token  

#### Request
Path parameter: `quizId` (GUID)

#### Response (200 OK)
```json
{
  "quiz": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "title": "80s Rock Bands Quiz",
    "userPrompt": "Create a quiz about 80s rock bands and their hit songs",
    "format": "MultipleChoice",
    "difficulty": "Medium",
    "questionCount": 10,
    "createdAt": "2025-09-15T12:00:00Z",
    "expiresAt": "2025-09-16T12:00:00Z",
    "status": "Generated",
    "questions": [...] // Same structure as generate response
  }
}
```

#### Response (404 Not Found)
```json
{
  "error": "quiz_not_found",
  "message": "Quiz not found or has expired",
  "correlationId": "abc-123-def"
}
```

---

### GET /api/quiz/my-quizzes
**Purpose**: Get user's quiz history with pagination  
**Authentication**: Bearer Token  

#### Request Query Parameters
- `page`: Optional, default 1
- `pageSize`: Optional, range 1-50, default 10
- `status`: Optional, filter by status ["Generated", "InProgress", "Completed", "Expired"]
- `sortBy`: Optional, ["CreatedAt", "Title"], default "CreatedAt"
- `sortOrder`: Optional, ["Asc", "Desc"], default "Desc"

#### Response (200 OK)
```json
{
  "quizzes": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "title": "80s Rock Bands Quiz",
      "userPrompt": "Create a quiz about 80s rock bands...",
      "format": "MultipleChoice",
      "difficulty": "Medium",
      "questionCount": 10,
      "createdAt": "2025-09-15T12:00:00Z",
      "expiresAt": "2025-09-16T12:00:00Z",
      "status": "Generated"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalItems": 25,
    "totalPages": 3,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

---

### DELETE /api/quiz/{quizId}
**Purpose**: Delete a specific quiz (early cleanup)  
**Authentication**: Bearer Token  

#### Request
Path parameter: `quizId` (GUID)

#### Response (204 No Content)
No response body.

#### Response (404 Not Found)
```json
{
  "error": "quiz_not_found",
  "message": "Quiz not found or already deleted",
  "correlationId": "abc-123-def"
}
```

---

### POST /api/quiz/{quizId}/start-session
**Purpose**: Start a new quiz session for taking the quiz  
**Authentication**: Bearer Token  

#### Request
```json
{
  "deviceId": "optional-spotify-device-id"
}
```

#### Response (200 OK)
```json
{
  "session": {
    "id": "660e8400-e29b-41d4-a716-446655440000",
    "quizId": "550e8400-e29b-41d4-a716-446655440000",
    "startedAt": "2025-09-15T13:00:00Z",
    "currentQuestionIndex": 0,
    "status": "InProgress",
    "totalScore": 0,
    "maxPossibleScore": 10,
    "selectedDevice": {
      "spotifyDeviceId": "device-123",
      "name": "My Computer",
      "type": "Computer",
      "isActive": true
    }
  }
}
```

#### Response (409 Conflict)
```json
{
  "error": "session_limit_exceeded",
  "message": "Maximum number of active sessions reached (3)",
  "correlationId": "abc-123-def",
  "details": {
    "activeSessions": 3,
    "maxSessions": 3
  }
}
```

---

### POST /api/quiz/validate-tracks
**Purpose**: Validate Spotify track availability for testing  
**Authentication**: Bearer Token  

#### Request
```json
{
  "trackIds": [
    "4VqPOruhp5EdPBeR92t6lQ",
    "7qiZfU4dY1lWllzX7mPBI3"
  ]
}
```

#### Response (200 OK)
```json
{
  "results": [
    {
      "trackId": "4VqPOruhp5EdPBeR92t6lQ",
      "isPlayable": true,
      "isAvailable": true,
      "track": {
        "name": "Don't Stop Believin'",
        "artistName": "Journey",
        "albumName": "Escape",
        "durationMs": 251000,
        "previewUrl": "https://p.scdn.co/mp3-preview/..."
      }
    },
    {
      "trackId": "7qiZfU4dY1lWllzX7mPBI3",
      "isPlayable": false,
      "isAvailable": false,
      "error": "Track not available in user's region"
    }
  ],
  "summary": {
    "totalTracks": 2,
    "playableTracks": 1,
    "unavailableTracks": 1
  }
}
```

## Quiz Generation Process

### Content Generation Flow
1. **Prompt Analysis**: AI analyzes user prompt for music topics, genres, artists
2. **Question Generation**: AI creates questions based on prompt context
3. **Track Search**: System searches Spotify for relevant tracks
4. **Track Validation**: Verify tracks are playable in user's region
5. **Content Assembly**: Combine questions with validated tracks
6. **Quality Check**: Ensure appropriate content and answer accuracy

### Performance Targets
- **Generation Time**: <5 seconds for 10-question quiz
- **Track Match Rate**: >80% of questions with audio when requested
- **Content Quality**: AI-generated content filtered for appropriateness

### Error Handling
- **AI Service Failure**: Fallback to text-only questions
- **Track Unavailability**: Alternative tracks or text-only format
- **Rate Limits**: Queue requests and provide estimated wait time
- **Content Issues**: Manual review flag for inappropriate content

## Rate Limiting

### Limits per User
- **Quiz Generation**: 10 per hour, 50 per day
- **Quiz Retrieval**: 300 per hour
- **Track Validation**: 100 per hour

### Rate Limit Response (429 Too Many Requests)
```json
{
  "error": "rate_limit_exceeded",
  "message": "Quiz generation limit exceeded. Try again in 1 hour.",
  "correlationId": "abc-123-def",
  "retryAfter": 3600
}
```