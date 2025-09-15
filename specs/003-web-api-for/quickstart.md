# VibeGuess API Quickstart Guide

**Version**: 1.0.0  
**Date**: 2025-09-15  
**Audience**: Developers integrating with VibeGuess API

## Overview

The VibeGuess API enables developers to create AI-powered music quizzes with integrated Spotify playback. This guide walks through the complete workflow from authentication to quiz creation and playback control.

## Prerequisites

### Required Accounts & Setup
1. **Spotify Developer Account**: [Create at developer.spotify.com](https://developer.spotify.com)
   - Register your application
   - Obtain Client ID and Client Secret
   - Configure redirect URIs for OAuth flow

2. **VibeGuess API Access**: 
   - API Base URL: `https://api.vibeguess.com`
   - Request API credentials from VibeGuess team

3. **User Requirements**:
   - Spotify account (Free or Premium)
   - Active Spotify device for playback testing

## Quick Start (5 Minutes)

### Step 1: User Authentication
Initialize Spotify OAuth flow to authenticate users.

```bash
# 1.1 Start OAuth flow
curl -X POST https://api.vibeguess.com/api/auth/spotify/login \
  -H "Content-Type: application/json" \
  -d '{
    "redirectUri": "https://your-app.com/callback",
    "state": "random-state-string"
  }'
```

**Response**:
```json
{
  "authorizationUrl": "https://accounts.spotify.com/authorize?client_id=...",
  "codeVerifier": "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk",
  "state": "random-state-string"
}
```

```bash
# 1.2 Complete authentication (after user authorizes)
curl -X POST https://api.vibeguess.com/api/auth/spotify/callback \
  -H "Content-Type: application/json" \
  -d '{
    "code": "authorization-code-from-spotify",
    "codeVerifier": "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk",
    "redirectUri": "https://your-app.com/callback"
  }'
```

**Response**:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh-token-string",
  "expiresIn": 3600,
  "user": {
    "id": "spotify-user-id",
    "displayName": "John Doe",
    "hasSpotifyPremium": true
  }
}
```

### Step 2: Generate a Music Quiz
Create an AI-powered quiz based on a text prompt.

```bash
# 2.1 Generate quiz
curl -X POST https://api.vibeguess.com/api/quiz/generate \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Create a quiz about 80s rock bands and their hit songs",
    "questionCount": 5,
    "format": "MultipleChoice",
    "includeAudio": true
  }'
```

**Response**:
```json
{
  "quiz": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "title": "80s Rock Bands Quiz",
    "questionCount": 5,
    "questions": [
      {
        "id": "question-1",
        "orderIndex": 1,
        "questionText": "Which band released 'Don't Stop Believin'' in 1981?",
        "track": {
          "spotifyTrackId": "4VqPOruhp5EdPBeR92t6lQ",
          "name": "Don't Stop Believin'",
          "artistName": "Journey"
        },
        "answerOptions": [
          {"optionText": "Journey", "isCorrect": true},
          {"optionText": "Foreigner", "isCorrect": false},
          {"optionText": "REO Speedwagon", "isCorrect": false}
        ]
      }
    ]
  }
}
```

### Step 3: Set Up Playback Control
Get available devices and start a quiz session with playback.

```bash
# 3.1 Get available Spotify devices
curl -X GET https://api.vibeguess.com/api/playback/devices \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Response**:
```json
{
  "devices": [
    {
      "spotifyDeviceId": "device-123",
      "name": "My Computer",
      "type": "Computer",
      "isActive": true,
      "volumePercent": 75
    }
  ]
}
```

```bash
# 3.2 Start quiz session with device selection
curl -X POST https://api.vibeguess.com/api/quiz/550e8400-e29b-41d4-a716-446655440000/start-session \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "deviceId": "device-123"
  }'
```

**Response**:
```json
{
  "session": {
    "id": "session-456",
    "quizId": "550e8400-e29b-41d4-a716-446655440000",
    "status": "InProgress",
    "currentQuestionIndex": 0,
    "selectedDevice": {
      "spotifyDeviceId": "device-123",
      "name": "My Computer"
    }
  }
}
```

### Step 4: Control Music Playback
Play tracks associated with quiz questions.

```bash
# 4.1 Play track for current question
curl -X POST https://api.vibeguess.com/api/playback/play \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "trackId": "4VqPOruhp5EdPBeR92t6lQ",
    "sessionId": "session-456",
    "positionMs": 0
  }'
```

**Response**:
```json
{
  "playback": {
    "isPlaying": true,
    "track": {
      "name": "Don't Stop Believin'",
      "artistName": "Journey"
    },
    "device": {
      "name": "My Computer"
    }
  }
}
```

```bash
# 4.2 Pause playback
curl -X POST https://api.vibeguess.com/api/playback/pause \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "session-456"
  }'
```

## Complete Example Application

### JavaScript/Node.js Integration

```javascript
class VibeGuessClient {
  constructor(baseUrl = 'https://api.vibeguess.com') {
    this.baseUrl = baseUrl;
    this.accessToken = null;
  }

  // Step 1: Authentication
  async startLogin(redirectUri) {
    const response = await fetch(`${this.baseUrl}/api/auth/spotify/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ redirectUri })
    });
    return response.json();
  }

  async completeLogin(code, codeVerifier, redirectUri) {
    const response = await fetch(`${this.baseUrl}/api/auth/spotify/callback`, {
      method: 'POST', 
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ code, codeVerifier, redirectUri })
    });
    const result = await response.json();
    this.accessToken = result.accessToken;
    return result;
  }

  // Step 2: Quiz Generation
  async generateQuiz(prompt, options = {}) {
    const response = await fetch(`${this.baseUrl}/api/quiz/generate`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.accessToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        prompt,
        questionCount: options.questionCount || 10,
        format: options.format || 'MultipleChoice',
        includeAudio: options.includeAudio !== false
      })
    });
    return response.json();
  }

  // Step 3: Device Management
  async getDevices() {
    const response = await fetch(`${this.baseUrl}/api/playback/devices`, {
      headers: { 'Authorization': `Bearer ${this.accessToken}` }
    });
    return response.json();
  }

  async startQuizSession(quizId, deviceId) {
    const response = await fetch(`${this.baseUrl}/api/quiz/${quizId}/start-session`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.accessToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ deviceId })
    });
    return response.json();
  }

  // Step 4: Playback Control
  async playTrack(trackId, sessionId, deviceId = null) {
    const response = await fetch(`${this.baseUrl}/api/playback/play`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.accessToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ trackId, sessionId, deviceId })
    });
    return response.json();
  }

  async pausePlayback(sessionId) {
    const response = await fetch(`${this.baseUrl}/api/playback/pause`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.accessToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ sessionId })
    });
    return response.json();
  }
}

// Usage Example
async function createMusicQuiz() {
  const client = new VibeGuessClient();
  
  try {
    // 1. Authenticate user (OAuth flow)
    const loginData = await client.startLogin('https://your-app.com/callback');
    console.log('Redirect user to:', loginData.authorizationUrl);
    
    // After user completes OAuth...
    // await client.completeLogin(authCode, loginData.codeVerifier, redirectUri);
    
    // 2. Generate quiz
    const quiz = await client.generateQuiz(
      "Create a quiz about indie rock bands from the 2000s",
      { questionCount: 8, includeAudio: true }
    );
    console.log('Quiz generated:', quiz.quiz.title);
    
    // 3. Set up playback
    const devices = await client.getDevices();
    const activeDevice = devices.devices.find(d => d.isActive);
    
    const session = await client.startQuizSession(quiz.quiz.id, activeDevice.spotifyDeviceId);
    console.log('Session started:', session.session.id);
    
    // 4. Play first question's track
    const firstQuestion = quiz.quiz.questions[0];
    if (firstQuestion.track) {
      await client.playTrack(
        firstQuestion.track.spotifyTrackId, 
        session.session.id
      );
      console.log('Playing:', firstQuestion.track.name);
      
      // Pause after 30 seconds
      setTimeout(() => {
        client.pausePlayback(session.session.id);
        console.log('Playback paused');
      }, 30000);
    }
    
  } catch (error) {
    console.error('Error:', error);
  }
}
```

## Testing & Development

### Health Checks
Verify API functionality before integration:

```bash
# Basic health check
curl https://api.vibeguess.com/api/health

# Detailed system status
curl https://api.vibeguess.com/api/health/detailed
```

### Test Endpoints
Use test endpoints to validate integration:

```bash
# Test authentication flow
curl -X POST https://api.vibeguess.com/api/health/test/auth \
  -H "Authorization: Bearer your-token" \
  -H "Content-Type: application/json" \
  -d '{"testUserId": "test-user-123"}'

# Test quiz generation
curl -X POST https://api.vibeguess.com/api/health/test/quiz-generation \
  -H "Authorization: Bearer your-token" \
  -H "Content-Type: application/json" \
  -d '{"testPrompt": "Test rock quiz", "mockMode": true}'

# Test playback functionality  
curl -X POST https://api.vibeguess.com/api/health/test/playback \
  -H "Authorization: Bearer your-token" \
  -H "Content-Type: application/json" \
  -d '{"testTrackId": "4VqPOruhp5EdPBeR92t6lQ", "mockDevice": true}'
```

## Error Handling

### Common Error Responses
Handle these standard error formats in your application:

```json
// Authentication Error
{
  "error": "unauthorized",
  "message": "Invalid or expired token",
  "correlationId": "abc-123-def"
}

// Rate Limit Error
{
  "error": "rate_limit_exceeded", 
  "message": "Too many requests. Try again in 1 hour.",
  "retryAfter": 3600
}

// External Service Error
{
  "error": "spotify_unavailable",
  "message": "Spotify API temporarily unavailable",
  "retryAfter": 60
}
```

### Error Handling Best Practices

```javascript
async function handleApiCall(apiFunction) {
  try {
    return await apiFunction();
  } catch (error) {
    if (error.status === 401) {
      // Token expired - refresh or re-authenticate
      await refreshToken();
      return apiFunction(); // Retry
    } else if (error.status === 429) {
      // Rate limited - wait and retry
      const retryAfter = error.headers['retry-after'] || 60;
      await new Promise(resolve => setTimeout(resolve, retryAfter * 1000));
      return apiFunction();
    } else if (error.status === 503) {
      // Service unavailable - exponential backoff
      throw new Error('Service temporarily unavailable');
    }
    throw error;
  }
}
```

## Rate Limits & Best Practices

### API Rate Limits
- **Authentication**: 5 login attempts per minute per IP
- **Quiz Generation**: 10 per hour, 50 per day per user
- **Playback Control**: 120 per minute (2 per second) per user
- **Device Operations**: 60 per minute per user

### Performance Optimization
1. **Cache quiz data** for session duration
2. **Batch track validation** when possible
3. **Implement exponential backoff** for failed requests
4. **Use correlation IDs** for debugging
5. **Monitor response times** and set appropriate timeouts

### Security Considerations
1. **Store tokens securely** (encrypted, secure storage)
2. **Implement CSRF protection** for OAuth state parameter
3. **Validate all input** on client side before API calls
4. **Use HTTPS only** for all API communications
5. **Implement proper session management**

## Next Steps

### Advanced Features
- **Custom Quiz Formats**: Extend beyond multiple choice and free text
- **User Analytics**: Track quiz performance and preferences  
- **Social Features**: Share quizzes and compete with friends
- **Playlist Integration**: Generate quizzes from Spotify playlists
- **Real-time Multiplayer**: Synchronize quiz sessions across multiple users

### Integration Support
- **Documentation**: Full API reference at [docs.vibeguess.com](https://docs.vibeguess.com)
- **SDKs**: Official SDKs for JavaScript, Python, C#, Swift
- **Support**: Technical support at [support@vibeguess.com](mailto:support@vibeguess.com)
- **Community**: Developer community at [github.com/vibeguess](https://github.com/vibeguess)

---

**🎵 Ready to create amazing music quiz experiences!**

For complete API reference, visit our [full documentation](https://docs.vibeguess.com).  
Questions? Reach out to our [developer community](https://github.com/vibeguess/community).