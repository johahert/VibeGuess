# Quiz API

Base URL: `/api/quiz`

Authentication: Bearer token unless noted. Public quizzes can be retrieved anonymously.

Headers used
- X-Correlation-ID: Optional request header. If provided, echoed back in responses.
- X-RateLimit-Limit / X-RateLimit-Remaining: Included on select endpoints (generate, my-quizzes).
- ETag: Returned by GET /api/quiz/{quizId}. If-None-Match supported.

## Endpoints

### POST /api/quiz/generate
Purpose: Generate a new music quiz from a user prompt

Auth: Required

Request
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

Validation
- prompt: required, 10–1000 chars (trimmed)
- questionCount: 5–20 (default 10)
- format: MultipleChoice | FreeText | Mixed (default MultipleChoice)
- difficulty: Easy | Medium | Hard (default Medium)
- includeAudio: boolean (default true)
- language: ISO 639-1 code (default en)

Responses
- 200 OK
  - Body wraps the quiz in a top-level `quiz` object and may include `generationMetadata`.
  - Example (truncated for brevity):
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
        "expiresAt": "2025-10-15T12:00:00Z",
        "status": "Generated",
        "questions": [
          {
            "id": "...",
            "orderIndex": 1,
            "questionText": "...",
            "type": "MultipleChoice",
            "requiresAudio": true,
            "points": 1,
            "hint": "...",
            "track": {
              "spotifyTrackId": "...",
              "name": "...",
              "artistName": "...",
              "albumName": "...",
              "durationMs": 0,
              "previewUrl": "...",
              "isPlayable": true
            },
            "answerOptions": [
              { "id": "...", "orderIndex": 1, "optionText": "...", "isCorrect": true }
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
- 400 Bad Request: `invalid_request` plus details
- 401 Unauthorized: `invalid_token`
- 500 Internal Server Error: `internal_server_error`

Notes
- Rate limit headers are included: `X-RateLimit-Limit: 10`, `X-RateLimit-Remaining`.
- X-Correlation-ID is echoed if sent.

---

### GET /api/quiz/{quizId}
Purpose: Retrieve a specific quiz by ID

Auth: Optional (required only for private quizzes unless owner)

Path params
- quizId: GUID

Caching
- Supports ETag via If-None-Match. Returns 304 when matched. ETag header is included in 200 responses.

Responses
- 200 OK
  - Body returns the quiz object directly (no `quiz` wrapper):
    ```json
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "title": "80s Rock Bands Quiz",
      "userPrompt": "...",
      "format": "MultipleChoice",
      "difficulty": "Medium",
      "questionCount": 10,
      "createdAt": "2025-09-15T12:00:00Z",
      "expiresAt": "2025-10-15T12:00:00Z",
      "status": "Generated",
      "description": "A medium difficulty quiz about music",
      "estimatedDuration": 600,
      "userProgress": { "completed": false, "score": null, "currentQuestion": 0 },
      "canEdit": false,
      "isBookmarked": false,
      "isPublic": true,
      "playCount": 0,
      "averageScore": 0,
      "tags": ["rock", "80s"],
      "questions": [
        {
          "id": "...",
          "orderIndex": 1,
          "questionText": "...",
          "type": "MultipleChoice",
          "requiresAudio": true,
          "points": 1,
          "hint": "...",
          "explanation": "...",
          "track": {
            "spotifyTrackId": "...",
            "name": "...",
            "artistName": "...",
            "albumName": "...",
            "durationMs": 0,
            "previewUrl": "...",
            "albumImageUrl": "...",
            "isPlayable": true
          },
          "answerOptions": [
            { "id": "...", "orderIndex": 1, "optionText": "...", "isCorrect": true }
          ]
        }
      ]
    }
    ```
- 304 Not Modified (ETag matched)
- 400 Bad Request: `invalid_quiz_id`
- 401 Unauthorized: when accessing a private quiz without auth
- 403 Forbidden: when authenticated but not owner of a private quiz
- 404 Not Found: `not_found` (quiz not found or expired)

Notes
- X-Correlation-ID is echoed if sent.

---

### GET /api/quiz/my-quizzes
Purpose: Get the authenticated user's quiz history

Auth: Required

Query params
- page: default 1
- pageSize: 1–50 (default 10)
- status: optional
- difficulty: optional
- sortBy: CreatedAt | Title (default CreatedAt)
- sortOrder: Asc | Desc (default Desc)

Responses
- 200 OK
  - If pagination params are present, returns an object with pagination:
    ```json
    {
      "quizzes": [
        {
          "id": "...",
          "title": "...",
          "userPrompt": "...",
          "format": "MultipleChoice",
          "difficulty": "Medium",
          "questionCount": 10,
          "questionsCount": 10,
          "createdAt": "2025-09-15T12:00:00Z",
          "expiresAt": "2025-10-15T12:00:00Z",
          "status": "Generated",
          "playCount": 0,
          "averageScore": 0,
          "isPublic": true,
          "tags": ["rock", "80s"]
        }
      ],
      "totalCount": 25,
      "page": 1,
      "pageSize": 10,
      "hasNextPage": true,
      "hasPreviousPage": false,
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
  - If no pagination params, returns a simple array of quizzes.
- 400 Bad Request: `invalid_request` (pagination/sort params)
- 401 Unauthorized: `invalid_token`

Notes
- Rate limit headers are included: `X-RateLimit-Limit: 300`.
- X-Correlation-ID is echoed if sent.

---

### PUT /api/quiz/{quizId}
Purpose: Update quiz metadata (owner only)

Auth: Required (must be the quiz owner)

Path params
- quizId: GUID

Request
```json
{
  "title": "New title",
  "difficulty": "Easy",
  "format": "Mixed",
  "isPublic": true,
  "includesAudio": true,
  "status": "Generated",
  "language": "en",
  "tags": ["tag1", "tag2"]
}
```

Validation
- title: 3–200 chars (optional)
- difficulty: one of current system values
- format: one of current system values
- status: one of current system values
- language: 2–5 chars (ISO 639-1)
- tags: combined length <= 500

Responses
- 200 OK
  - Body wraps the updated quiz in a top-level `quiz` object (same shape as generate response).
- 400 Bad Request: `invalid_request`
- 401 Unauthorized: `invalid_token`
- 403 Forbidden: `forbidden` (not the owner)
- 404 Not Found: `quiz_not_found`

Notes
- X-Correlation-ID is echoed if sent.

---

### DELETE /api/quiz/{quizId}
Purpose: Delete a quiz (owner only)

Auth: Required (must be the quiz owner)

Path params
- quizId: GUID

Responses
- 204 No Content
- 400 Bad Request: `invalid_quiz_id`
- 401 Unauthorized: `invalid_token`
- 403 Forbidden: `forbidden` (not the owner)
- 404 Not Found: `quiz_not_found`

Notes
- Also clears any in-memory active session references for the quiz.
- X-Correlation-ID is echoed if sent.

---

### POST /api/quiz/{quizId}/start-session
Purpose: Start a new quiz session

Auth: Required

Path params
- quizId: GUID

Request
```json
{ "deviceId": "spotify-device-id" }
```

Responses
- 201 Created
  - Location: `/api/quiz/session/{sessionId}`
  - Body (top-level session info):
    ```json
    {
      "sessionId": "660e8400-e29b-41d4-a716-446655440000",
      "quizId": "550e8400-e29b-41d4-a716-446655440000",
      "startedAt": "2025-09-15T13:00:00Z",
      "currentQuestionIndex": 0,
      "totalQuestions": 10,
      "status": "active",
      "totalScore": 0,
      "maxPossibleScore": 10,
      "selectedDevice": {
        "spotifyDeviceId": "device-123",
        "name": "Test Device",
        "type": "Computer",
        "isActive": true
      }
    }
    ```
- 400 Bad Request
  - `missing_deviceid` when deviceId is absent
  - `invalid_json` when request body cannot be parsed
  - `invalid_device` when deviceId is malformed/unknown
  - `missing_request_body` when the request body is empty
- 404 Not Found: `quiz not found` (quiz does not exist)
- 409 Conflict: `active session` (session already exists for this quiz)

Notes
- X-Correlation-ID is echoed if sent.

---

## Additional Notes

- Content generation, Spotify track search/validation, and quality checks happen server-side; responses include only validated tracks.
- Performance targets and operational guarantees may vary; handle 500 errors with retries or surfaced messages.

## Rate Limiting

Current limits (subject to change)
- Quiz generation: 10/hour (headers included on responses)
- Quiz retrieval (my-quizzes): 300/hour (headers included on responses)

429 Too Many Requests
```json
{
  "error": "rate_limit_exceeded",
  "message": "Limit exceeded. Try again later.",
  "correlationId": "abc-123-def",
  "retryAfter": 3600
}
```