# Spotify Data API Endpoints

**Base URL**: `/api/spotify`

**Authentication**: Bearer token recommended. Some endpoints require a valid Spotify user token to succeed (devices). Rate limit and correlation headers are returned via `X-RateLimit-*` and `X-Correlation-ID` when available.

## GET /api/spotify/search
**Purpose**: Find a single track by specifying both track and artist names.

**Query Parameters**
- `trackName` (string, required)
- `artistName` (string, required)

**Responses**
- `200 OK`
  ```json
  {
    "found": true,
    "track": {
      "id": "d5f7b13b-2de5-4b6f-8c7f-8469a7a2f0ef",
      "spotifyTrackId": "4VqPOruhp5EdPBeR92t6lQ",
      "name": "Don't Stop Believin'",
      "artistName": "Journey",
      "allArtists": "Journey",
      "albumName": "Escape",
      "durationMs": 251000,
      "popularity": 78,
      "isExplicit": false,
      "previewUrl": "https://p.scdn.co/mp3-preview/...",
      "spotifyUrl": "https://open.spotify.com/track/4VqPOruhp5EdPBeR92t6lQ",
      "albumImageUrl": "https://i.scdn.co/image/...",
      "releaseDate": "1981-07-17T00:00:00",
      "createdAt": "2025-09-27T15:30:12Z"
    },
    "message": "Track found successfully"
  }
  ```
- `200 OK` (not found)
  ```json
  {
    "found": false,
    "track": null,
    "message": "No track found for 'Journey - Lights'"
  }
  ```
- `400 Bad Request`
  ```json
  {
    "error": "invalid_request",
    "message": "Track name is required",
    "correlationId": "abc-123-def"
  }
  ```
- `500 Internal Server Error`
  ```json
  {
    "error": "search_error",
    "message": "An error occurred while searching for the track",
    "correlationId": "abc-123-def"
  }
  ```

---

## GET /api/spotify/search/tracks
**Purpose**: Search Spotify using a single free-form query and return up to 10 matching tracks.

**Query Parameters**
- `query` (string, required) – A track/artist/album search string. Minimum 1 non-whitespace character.
- `limit` (int, optional) – Number of tracks to return, clamped between 1 and 10. Default 10.

**Responses**
- `200 OK`
  ```json
  {
    "query": "journey don't stop",
    "limit": 5,
    "count": 5,
    "tracks": [
      {
        "id": "d5f7b13b-2de5-4b6f-8c7f-8469a7a2f0ef",
        "spotifyTrackId": "4VqPOruhp5EdPBeR92t6lQ",
        "name": "Don't Stop Believin'",
        "artistName": "Journey",
        "allArtists": "Journey",
        "albumName": "Escape",
        "durationMs": 251000,
        "popularity": 78,
        "isExplicit": false,
        "previewUrl": "https://p.scdn.co/mp3-preview/...",
        "spotifyUrl": "https://open.spotify.com/track/4VqPOruhp5EdPBeR92t6lQ",
        "albumImageUrl": "https://i.scdn.co/image/...",
        "releaseDate": "1981-07-17T00:00:00",
        "createdAt": "2025-09-27T15:30:12Z"
      }
    ]
  }
  ```
- `200 OK` (no matches)
  ```json
  {
    "query": "unknown track",
    "limit": 10,
    "count": 0,
    "tracks": []
  }
  ```
- `400 Bad Request`
  ```json
  {
    "error": "invalid_request",
    "message": "Query is required",
    "correlationId": "abc-123-def"
  }
  ```
- `500 Internal Server Error`
  ```json
  {
    "error": "search_error",
    "message": "Spotify search is currently unavailable",
    "correlationId": "abc-123-def"
  }
  ```

---

## GET /api/spotify/track/{spotifyTrackId}
**Purpose**: Retrieve full details for a Spotify track by ID.

**Path Parameters**
- `spotifyTrackId` (string, required)

**Responses**
- `200 OK`
  ```json
  {
    "track": {
      "id": "d5f7b13b-2de5-4b6f-8c7f-8469a7a2f0ef",
      "spotifyTrackId": "4VqPOruhp5EdPBeR92t6lQ",
      "name": "Don't Stop Believin'",
      "artistName": "Journey",
      "allArtists": "Journey",
      "albumName": "Escape",
      "durationMs": 251000,
      "popularity": 78,
      "isExplicit": false,
      "previewUrl": "https://p.scdn.co/mp3-preview/...",
      "spotifyUrl": "https://open.spotify.com/track/4VqPOruhp5EdPBeR92t6lQ",
      "albumImageUrl": "https://i.scdn.co/image/...",
      "releaseDate": "1981-07-17T00:00:00",
      "createdAt": "2025-09-27T15:30:12Z"
    }
  }
  ```
- `400 Bad Request`
  ```json
  {
    "error": "invalid_request",
    "message": "Spotify track ID is required",
    "correlationId": "abc-123-def"
  }
  ```
- `404 Not Found`
  ```json
  {
    "message": "Track with Spotify ID '123' not found"
  }
  ```
- `500 Internal Server Error`
  ```json
  {
    "error": "track_error",
    "message": "An error occurred while retrieving the track",
    "correlationId": "abc-123-def"
  }
  ```

---

## GET /api/spotify/devices
**Purpose**: List the authenticated user's Spotify Connect devices.

**Responses**
- `200 OK`
  ```json
  {
    "devices": [
      {
        "id": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
        "name": "My Computer",
        "type": "Computer",
        "isActive": true,
        "isPrivateSession": false,
        "isRestricted": false,
        "volumePercent": 75,
        "supportsVolume": true
      }
    ],
    "activeDevice": {
      "id": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
      "name": "My Computer",
      "type": "Computer",
      "isActive": true,
      "isPrivateSession": false,
      "isRestricted": false,
      "volumePercent": 75,
      "supportsVolume": true
    },
    "deviceCount": 1,
    "activeDeviceCount": 1
  }
  ```
- `401 Unauthorized`
  ```json
  {
    "error": "authentication_required",
    "message": "User must be authenticated with Spotify to access devices",
    "correlationId": "abc-123-def"
  }
  ```
- `500 Internal Server Error`
  ```json
  {
    "error": "devices_error",
    "message": "An error occurred while retrieving devices",
    "correlationId": "abc-123-def"
  }
  ```

---

## Notes
- `X-Correlation-ID` headers are echoed when provided.
- Rate limiting headers may be included for observability but are simulated in the current implementation.
- Track DTOs match the structure returned by quiz generation and playback services, enabling reuse on the frontend.
