# API Authentication Endpoints

**Base URL**: `/api/auth`  
**Version**: v1  
**Authentication**: None (these endpoints provide authentication)

## Endpoints Overview

### POST /api/auth/spotify/login
**Purpose**: Initiate Spotify OAuth 2.0 PKCE flow  
**Authentication**: None  

#### Request
```json
{
  "redirectUri": "https://your-app.com/callback",
  "state": "optional-state-parameter"
}
```

#### Response (200 OK)
```json
{
  "authorizationUrl": "https://accounts.spotify.com/authorize?client_id=...",
  "codeVerifier": "base64-encoded-code-verifier",
  "state": "state-parameter"
}
```

#### Response (400 Bad Request)
```json
{
  "error": "invalid_request",
  "message": "Invalid redirect URI format",
  "correlationId": "abc-123-def"
}
```

---

### POST /api/auth/spotify/callback
**Purpose**: Complete OAuth flow and obtain tokens  
**Authentication**: None  

#### Request
```json
{
  "code": "authorization-code-from-spotify",
  "codeVerifier": "base64-encoded-code-verifier",
  "redirectUri": "https://your-app.com/callback"
}
```

#### Response (200 OK)
```json
{
  "accessToken": "jwt-access-token",
  "refreshToken": "refresh-token",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "user": {
    "id": "spotify-user-id",
    "displayName": "User Display Name",
    "email": "user@example.com",
    "hasSpotifyPremium": true,
    "country": "US"
  }
}
```

#### Response (400 Bad Request)
```json
{
  "error": "invalid_grant",
  "message": "Invalid authorization code or code verifier",
  "correlationId": "abc-123-def"
}
```

---

### POST /api/auth/refresh
**Purpose**: Refresh expired access token  
**Authentication**: Refresh Token (in request body)  

#### Request
```json
{
  "refreshToken": "valid-refresh-token"
}
```

#### Response (200 OK)
```json
{
  "accessToken": "new-jwt-access-token",
  "refreshToken": "new-refresh-token",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

#### Response (401 Unauthorized)
```json
{
  "error": "invalid_grant",
  "message": "Invalid or expired refresh token",
  "correlationId": "abc-123-def"
}
```

---

### POST /api/auth/logout
**Purpose**: Invalidate user session and tokens  
**Authentication**: Bearer Token  

#### Request
```json
{
  "refreshToken": "refresh-token-to-invalidate"
}
```

#### Response (200 OK)
```json
{
  "message": "Successfully logged out",
  "correlationId": "abc-123-def"
}
```

#### Response (401 Unauthorized)
```json
{
  "error": "unauthorized",
  "message": "Invalid or expired token",
  "correlationId": "abc-123-def"
}
```

---

### GET /api/auth/me
**Purpose**: Get current user profile information  
**Authentication**: Bearer Token  

#### Request
No request body required.

#### Response (200 OK)
```json
{
  "user": {
    "id": "spotify-user-id",
    "displayName": "User Display Name",
    "email": "user@example.com",
    "hasSpotifyPremium": true,
    "country": "US",
    "createdAt": "2025-09-15T10:00:00Z",
    "lastLoginAt": "2025-09-15T12:00:00Z"
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

#### Response (401 Unauthorized)
```json
{
  "error": "unauthorized",
  "message": "Invalid or expired token",
  "correlationId": "abc-123-def"
}
```

## Request/Response Headers

### Standard Request Headers
```
Authorization: Bearer {jwt-access-token}
Content-Type: application/json
Accept: application/json
User-Agent: VibeGuess-Client/1.0
X-Correlation-ID: {optional-correlation-id}
```

### Standard Response Headers
```
Content-Type: application/json
X-Correlation-ID: {correlation-id}
X-RateLimit-Remaining: {remaining-requests}
X-RateLimit-Reset: {reset-timestamp}
```

## Error Response Schema

### Standard Error Format
```json
{
  "error": "error_code",
  "message": "Human-readable error description",
  "correlationId": "unique-request-identifier",
  "details": {
    "field": "validation-error-details"
  }
}
```

### Common Error Codes
- `invalid_request`: Malformed request data
- `invalid_grant`: Invalid OAuth codes or tokens
- `unauthorized`: Authentication required or failed
- `rate_limit_exceeded`: Too many requests
- `spotify_unavailable`: Spotify API temporarily unavailable
- `internal_error`: Server error occurred

## Rate Limiting

### Limits
- **Login attempts**: 5 per minute per IP
- **Token refresh**: 10 per minute per user
- **Profile requests**: 60 per minute per user

### Rate Limit Headers
```
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 59
X-RateLimit-Reset: 1694779200
```

## Security Considerations

### PKCE Flow Implementation
- Code verifier: 128 characters, base64url-encoded
- Code challenge: SHA256 hash of verifier, base64url-encoded
- State parameter: CSRF protection, minimum 32 characters

### Token Security
- Access tokens: JWT format, 1-hour expiration
- Refresh tokens: Secure random string, 30-day expiration
- Token rotation: New refresh token issued with each access token refresh

### Input Validation
- Redirect URI: Must match registered app URIs
- All inputs: Sanitized to prevent injection attacks
- Request size: Maximum 1MB payload