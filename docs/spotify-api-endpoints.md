# Spotify API Endpoints

## Overview

The VibeGuess API now includes endpoints for searching and retrieving Spotify tracks using the `SearchTrackAsync` function from the `SpotifyApiService`.

## Available Endpoints

### 1. Production Spotify Controller

**Base URL:** `/api/spotify`

#### Search Track
- **Endpoint:** `GET /api/spotify/search`
- **Purpose:** Search for a track on Spotify by track name and artist name
- **Parameters:**
  - `trackName` (query, required): The name of the track to search for
  - `artistName` (query, required): The name of the artist to search for
- **Response:** `TrackSearchResponse` object

**Example Request:**
```
GET /api/spotify/search?trackName=Shape of You&artistName=Ed Sheeran
```

**Example Response:**
```json
{
  "found": true,
  "track": {
    "id": "12345678-1234-1234-1234-123456789abc",
    "spotifyTrackId": "7qiZfU4dY1lWllzX7mPBI3",
    "name": "Shape of You",
    "artistName": "Ed Sheeran",
    "allArtists": "Ed Sheeran",
    "albumName": "÷ (Divide)",
    "durationMs": 233713,
    "popularity": 91,
    "isExplicit": false,
    "previewUrl": "https://p.scdn.co/mp3-preview/...",
    "spotifyUrl": "https://open.spotify.com/track/7qiZfU4dY1lWllzX7mPBI3",
    "albumImageUrl": "https://i.scdn.co/image/...",
    "releaseDate": "2017-01-06T00:00:00Z",
    "createdAt": "2025-09-25T18:00:00Z"
  },
  "message": "Track found successfully"
}
```

#### Get Track by Spotify ID
- **Endpoint:** `GET /api/spotify/track/{spotifyTrackId}`
- **Purpose:** Get detailed information about a specific track using its Spotify ID
- **Parameters:**
  - `spotifyTrackId` (path, required): The Spotify track ID
- **Response:** `TrackResponse` object

**Example Request:**
```
GET /api/spotify/track/7qiZfU4dY1lWllzX7mPBI3
```

### 2. Diagnostic/Testing Endpoints

**Base URL:** `/api/spotifydiagnostics`

#### Test Authentication
- **Endpoint:** `GET /api/spotifydiagnostics/test-authentication`
- **Purpose:** Test Spotify authentication and API integration by searching for a known popular track
- **Parameters:** None
- **Response:** Test result with success status and sample track data

#### Search Track (Diagnostic)
- **Endpoint:** `GET /api/spotifydiagnostics/search-track`
- **Purpose:** Test track search functionality with flexible query parsing
- **Parameters:**
  - `query` (query, required): Search query in format "Track Artist" or "Artist - Track"
- **Response:** Detailed diagnostic information including parsed parameters and results

**Example Request:**
```
GET /api/spotifydiagnostics/search-track?query=Ed Sheeran - Shape of You
```

## Response Models

### TrackSearchResponse
```csharp
{
  "found": boolean,        // Whether a track was found
  "track": TrackDto,       // Track details (if found)
  "message": string        // Response message
}
```

### TrackResponse
```csharp
{
  "track": TrackDto        // Track details
}
```

### TrackDto
```csharp
{
  "id": "guid",                    // Internal track ID
  "spotifyTrackId": "string",      // Spotify track ID
  "name": "string",                // Track name
  "artistName": "string",          // Primary artist name
  "allArtists": "string",          // All artists (comma-separated)
  "albumName": "string",           // Album name
  "durationMs": number,            // Duration in milliseconds
  "popularity": number,            // Spotify popularity (0-100)
  "isExplicit": boolean,           // Has explicit content
  "previewUrl": "string",          // 30-second preview URL
  "spotifyUrl": "string",          // Spotify web URL
  "albumImageUrl": "string",       // Album cover image URL
  "releaseDate": "datetime",       // Album release date
  "createdAt": "datetime"          // When recorded in our system
}
```

## Error Responses

All endpoints use standardized error responses:

```json
{
  "error": "error_code",
  "message": "Human readable error message",
  "correlationId": "request-trace-id"
}
```

Common error codes:
- `invalid_request` (400): Missing or invalid parameters
- `search_error` (500): Internal error during search
- `track_error` (500): Internal error during track retrieval

## Rate Limiting

All endpoints include rate limiting headers:
- `X-RateLimit-Remaining`: Requests remaining in current window
- `X-RateLimit-Reset`: When the rate limit window resets (Unix timestamp)
- `X-Correlation-ID`: Request correlation ID for tracking

## Authentication

These endpoints use Spotify's client credentials flow for authentication, which is handled automatically by the service. No user authentication is required for track search operations.

## Usage Notes

1. **Search Quality**: For better search results, provide accurate track and artist names
2. **Caching**: Found tracks are automatically cached in the database to improve performance
3. **Fallback**: If Spotify API is unavailable, the system may fall back to mock tracks
4. **Diagnostics**: Use the diagnostic endpoints for testing and troubleshooting
5. **Performance**: The search endpoint typically responds in 200-500ms depending on Spotify API response times