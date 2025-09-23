# API Reference — Auth (JWT)

This document explains the authentication endpoints and their responses. The API now issues an application JWT for authenticated sessions. The `accessToken` field in responses contains the application JWT (signed with the server secret) and should be used as the Bearer token in subsequent API calls.

## POST /api/auth/spotify/callback
Exchanges a Spotify authorization code for Spotify tokens, persists the user and tokens, and returns an application JWT.

Example success response:

{
  "accessToken": "<APPLICATION_JWT>",
  "refreshToken": "<SPOTIFY_REFRESH_TOKEN>",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "user": { /* user profile */ }
}

Notes:
- `accessToken` should be used as `Authorization: Bearer <APPLICATION_JWT>` in subsequent API requests.
- `refreshToken` is still the Spotify refresh token; it may be used to refresh Spotify tokens. The API's `/api/auth/refresh` endpoint will accept this refresh token and return a new application JWT.

## POST /api/auth/refresh
Accepts a JSON body with `refreshToken` and exchanges it with Spotify for new tokens. The server updates stored Spotify tokens and responds with a new application JWT in `accessToken`.

Example success response:

{
  "accessToken": "<NEW_APPLICATION_JWT>",
  "refreshToken": "<NEW_SPOTIFY_REFRESH_TOKEN>",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}

If the server cannot map the refresh token to a user (unexpected), it will fall back to returning the raw Spotify tokens in the same shape as older responses.

Security:
- The application JWT is signed with the server secret configured under `Jwt:Secret` in configuration. For production, store the secret in a secure secret store or environment variable and use strong key material. Rotate keys periodically.
# VibeGuess API Reference

> Complete API documentation for frontend development

**Base URL**: `https://localhost:7009/api`  
**Authentication**: Bearer Token (JWT)  
**Content-Type**: `application/json`

## 🔐 Authentication Endpoints

### 1. Initiate Spotify Login

**POST** `/auth/spotify/login`

Starts the Spotify OAuth 2.0 PKCE flow.

**Request Body:**
```json
{
  "redirectUri": "http://localhost:5087/api/auth/spotify/callback",
  "state": "optional-csrf-state"
}
```

**Response (200):**
```json
{
  "authorizationUrl": "https://accounts.spotify.com/authorize?client_id=...",
  "codeVerifier": "abc123...",
  "state": "string"
}
```

**Frontend Usage:**
1. Call this endpoint
2. Redirect user to `authorizationUrl`
3. Store `codeVerifier` securely for callback

---

### 2. Complete Spotify Callback

**POST** `/auth/spotify/callback`

Exchanges authorization code for tokens and user profile.

**Request Body:**
```json
{
  "code": "AQBJ-XcUNW...",
  "codeVerifier": "abc123...",
  "redirectUri": "http://localhost:5087/api/auth/spotify/callback",
  "state": "string"
}
```

**Response (200):**
```json
{
  "accessToken": "BQC6FX_L9...",
  "refreshToken": "AQC7j2k...",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "user": {
    "id": "spotify-user-123",
    "displayName": "John Doe",
    "email": "john@example.com",
    "country": "US",
    "hasSpotifyPremium": true,
    "profileImageUrl": "https://i.scdn.co/image/..."
  }
}
```

**Error Responses:**
- `400` - Invalid authorization code/verifier
- `500` - Internal server error

---

### 3. Refresh Access Token

**POST** `/auth/refresh`

Refreshes expired access tokens.

**Request Body:**
```json
{
  "refreshToken": "AQC7j2k..."
}
```

**Response (200):**
```json
{
  "accessToken": "new.access.token.jwt",
  "refreshToken": "new.refresh.token",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

**Error Responses:**
- `401` - Invalid/expired refresh token
- `400` - Malformed JSON

---

### 4. Get User Profile

**GET** `/auth/me`

**Headers:** `Authorization: Bearer <access_token>`

**Response (200):**
```json
{
  "user": {
    "id": "spotify-user-123",
    "displayName": "John Doe",
    "email": "john@example.com",
    "hasSpotifyPremium": true,
    "country": "US",
    "createdAt": "2025-09-15T10:00:00Z",
    "lastLoginAt": "2025-09-21T14:30:00Z"
  },
  "settings": {
    "preferredLanguage": "en",
    "enableAudioPreview": true,
    "defaultQuestionCount": 10,
    "defaultDifficulty": "Medium",
    "rememberDeviceSelection": false
  }
}
```

---

## 🎵 Quiz Management Endpoints

### 5. Generate AI Quiz

**POST** `/quiz/generate`

**Headers:** `Authorization: Bearer <access_token>`

**Request Body:**
```json
{
  "prompt": "Create a quiz about 80s rock bands",
  "questionCount": 10,
  "format": "MultipleChoice",
  "difficulty": "Medium",
  "includeAudio": true
}
```

**Response (201):**
```json
{
  "id": "quiz-123-456",
  "title": "80s Rock Bands Quiz",
  "description": "Test your knowledge of classic rock",
  "questionCount": 10,
  "difficulty": "Medium",
  "estimatedDuration": 600,
  "createdAt": "2025-09-21T14:30:00Z",
  "questions": [
    {
      "id": "q1",
      "text": "Which band released 'Don't Stop Believin'?",
      "type": "MultipleChoice",
      "options": ["Journey", "Foreigner", "Boston", "REO Speedwagon"],
      "correctAnswer": "Journey",
      "spotifyTrack": {
        "id": "4bHsxqR3GMrXTxEPLuK5ue",
        "name": "Don't Stop Believin'",
        "previewUrl": "https://p.scdn.co/mp3-preview/..."
      }
    }
  ]
}
```

---

### 6. Get Quiz by ID

**GET** `/quiz/{id}`

**Headers:** `Authorization: Bearer <access_token>` (optional for public quizzes)

**Response (200):**
```json
{
  "id": "quiz-123-456",
  "title": "80s Rock Bands Quiz", 
  "description": "Test your knowledge of classic rock",
  "questionCount": 10,
  "difficulty": "Medium",
  "createdBy": "user-123",
  "isPublic": true,
  "tags": ["rock", "80s", "music"],
  "createdAt": "2025-09-21T14:30:00Z"
}
```

---

### 7. Start Quiz Session

**POST** `/quiz/{id}/start-session`

**Headers:** `Authorization: Bearer <access_token>`

**Request Body:**
```json
{
  "deviceId": "spotify-device-123"
}
```

**Response (201):**
```json
{
  "sessionId": "session-789",
  "quizId": "quiz-123-456",
  "currentQuestionIndex": 0,
  "score": 0,
  "startedAt": "2025-09-21T14:30:00Z",
  "isActive": true
}
```

---

### 8. Get Quiz History

**GET** `/quiz/my-quizzes`

**Headers:** `Authorization: Bearer <access_token>`

**Query Parameters:**
- `page=1` (optional)
- `limit=10` (optional)

**Response (200):**
```json
{
  "quizzes": [
    {
      "id": "quiz-123",
      "title": "80s Rock Quiz",
      "completedAt": "2025-09-21T13:00:00Z",
      "score": 8,
      "totalQuestions": 10
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 10,
    "total": 25,
    "totalPages": 3
  }
}
```

---

## 🎶 Spotify Playback Endpoints

### 9. Get Playback Devices

**GET** `/playback/devices`

**Headers:** `Authorization: Bearer <access_token>`

**Response (200):**
```json
{
  "devices": [
    {
      "id": "device-123",
      "name": "John's iPhone",
      "type": "Smartphone",
      "isActive": true,
      "isPrivateSession": false,
      "isRestricted": false,
      "volumePercent": 75
    }
  ]
}
```

---

### 10. Control Playback

**POST** `/playback/play`

**Headers:** `Authorization: Bearer <access_token>`

**Request Body:**
```json
{
  "trackId": "spotify:track:4bHsxqR3GMrXTxEPLuK5ue",
  "deviceId": "device-123",
  "positionMs": 0
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Playback started successfully"
}
```

---

**POST** `/playback/pause`

**Headers:** `Authorization: Bearer <access_token>`

**Request Body:**
```json
{
  "deviceId": "device-123"
}
```

---

### 11. Get Playback Status

**GET** `/playback/status`

**Headers:** `Authorization: Bearer <access_token>`

**Response (200):**
```json
{
  "isPlaying": true,
  "trackId": "spotify:track:4bHsxqR3GMrXTxEPLuK5ue",
  "trackName": "Don't Stop Believin'",
  "artistName": "Journey",
  "progressMs": 45000,
  "durationMs": 251000,
  "device": {
    "id": "device-123",
    "name": "John's iPhone"
  }
}
```

---

## 🏥 Health Check Endpoints

### 12. API Health

**GET** `/health`

**Response (200):**
```json
{
  "status": "Healthy",
  "timestamp": "2025-09-21T14:30:00Z",
  "version": "1.0.0",
  "environment": "Development"
}
```

---

### 13. Test Spotify Connection

**POST** `/health/test/spotify`

**Headers:** `Authorization: Bearer <access_token>`

**Request Body:**
```json
{
  "includeExtendedDiagnostics": false
}
```

**Response (200):**
```json
{
  "service": "spotify",
  "status": "Connected",
  "responseTime": 245,
  "lastChecked": "2025-09-21T14:30:00Z"
}
```

---

## 🔗 Common Response Headers

All endpoints include these headers:
- `X-Correlation-ID`: Request tracking ID
- `X-RateLimit-Remaining`: API calls remaining
- `X-RateLimit-Reset`: Reset time (Unix timestamp)

## 🚨 Error Response Format

```json
{
  "error": "invalid_request",
  "message": "The request is missing required parameters",
  "correlationId": "abc-123-def"
}
```

**Common Error Codes:**
- `400` - Bad Request (invalid_request)
- `401` - Unauthorized (invalid_token)
- `403` - Forbidden (insufficient_scope)
- `404` - Not Found (not_found)
- `429` - Rate Limited (rate_limit_exceeded)
- `500` - Internal Error (internal_error)

---

## 🎯 Frontend Integration Examples

### React/TypeScript Example

```typescript
// API Client
class VibeGuessAPI {
  constructor(private baseUrl = 'https://localhost:7009/api') {}

  async login(redirectUri: string) {
    const response = await fetch(`${this.baseUrl}/auth/spotify/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ redirectUri })
    });
    return response.json();
  }

  async getUserProfile(token: string) {
    const response = await fetch(`${this.baseUrl}/auth/me`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    return response.json();
  }

  async generateQuiz(token: string, prompt: string) {
    const response = await fetch(`${this.baseUrl}/quiz/generate`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ 
        prompt,
        questionCount: 10,
        format: 'MultipleChoice',
        difficulty: 'Medium'
      })
    });
    return response.json();
  }
}
```

### Authentication Flow Example

```typescript
// 1. Start OAuth flow
const { authorizationUrl, codeVerifier } = await api.login(
  'http://localhost:3000/callback'
);

// 2. Store codeVerifier and redirect
localStorage.setItem('codeVerifier', codeVerifier);
window.location.href = authorizationUrl;

// 3. Handle callback (in your callback route)
const urlParams = new URLSearchParams(window.location.search);
const code = urlParams.get('code');
const storedVerifier = localStorage.getItem('codeVerifier');

const tokens = await api.callback(code, storedVerifier, redirectUri);
localStorage.setItem('accessToken', tokens.accessToken);
```

---

**🎵 Ready to build your VibeGuess frontend!**