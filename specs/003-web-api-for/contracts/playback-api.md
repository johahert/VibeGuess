# API Playback Control Endpoints

**Base URL**: `/api/playback`  
**Version**: v1  
**Authentication**: Bearer Token (required)

## Endpoints Overview

### GET /api/playback/devices
**Purpose**: Get user's available Spotify devices  
**Authentication**: Bearer Token  

#### Request
No request body required.

#### Response (200 OK)
```json
{
  "devices": [
    {
      "spotifyDeviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
      "name": "My Computer",
      "type": "Computer", 
      "isActive": true,
      "isRestricted": false,
      "volumePercent": 75,
      "lastSeen": "2025-09-15T13:00:00Z"
    },
    {
      "spotifyDeviceId": "b4be4d64f9b19839f92426c71d8d0cd4e8f25a3e",
      "name": "iPhone",
      "type": "Smartphone",
      "isActive": false,
      "isRestricted": false,
      "volumePercent": 50,
      "lastSeen": "2025-09-15T12:45:00Z"
    },
    {
      "spotifyDeviceId": "c3df2e8f4a2b5c1d9e7f0a3b8c6e4d2f1a5b9c7e",
      "name": "Living Room Speaker",
      "type": "Speaker",
      "isActive": false,
      "isRestricted": true,
      "volumePercent": null,
      "lastSeen": "2025-09-15T11:30:00Z"
    }
  ],
  "activeDevice": {
    "spotifyDeviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
    "name": "My Computer"
  }
}
```

#### Response (503 Service Unavailable)
```json
{
  "error": "spotify_unavailable",
  "message": "Spotify API is temporarily unavailable",
  "correlationId": "abc-123-def",
  "retryAfter": 60
}
```

---

### POST /api/playback/select-device
**Purpose**: Select a device for playback in current session  
**Authentication**: Bearer Token  

#### Request
```json
{
  "deviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
  "sessionId": "660e8400-e29b-41d4-a716-446655440000"
}
```

#### Request Validation
- `deviceId`: Required, valid Spotify device ID format
- `sessionId`: Required, valid GUID, must be active session owned by user

#### Response (200 OK)
```json
{
  "selectedDevice": {
    "spotifyDeviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
    "name": "My Computer",
    "type": "Computer",
    "isActive": true,
    "isRestricted": false,
    "volumePercent": 75
  },
  "session": {
    "id": "660e8400-e29b-41d4-a716-446655440000",
    "status": "InProgress"
  }
}
```

#### Response (400 Bad Request)
```json
{
  "error": "invalid_device",
  "message": "Device is not available or restricted for playback",
  "correlationId": "abc-123-def",
  "details": {
    "deviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
    "reason": "Device is restricted by Spotify Premium requirements"
  }
}
```

---

### POST /api/playback/play
**Purpose**: Play a specific track on selected device  
**Authentication**: Bearer Token  

#### Request
```json
{
  "trackId": "4VqPOruhp5EdPBeR92t6lQ",
  "sessionId": "660e8400-e29b-41d4-a716-446655440000",
  "deviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
  "positionMs": 0
}
```

#### Request Validation
- `trackId`: Required, valid Spotify track ID
- `sessionId`: Required, active session ID owned by user
- `deviceId`: Optional, uses session's selected device if not provided
- `positionMs`: Optional, track position in milliseconds, default 0

#### Response (200 OK)
```json
{
  "playback": {
    "isPlaying": true,
    "track": {
      "spotifyTrackId": "4VqPOruhp5EdPBeR92t6lQ",
      "name": "Don't Stop Believin'",
      "artistName": "Journey",
      "albumName": "Escape",
      "durationMs": 251000
    },
    "device": {
      "spotifyDeviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
      "name": "My Computer"
    },
    "progressMs": 0,
    "volumePercent": 75,
    "timestamp": "2025-09-15T13:05:00Z"
  },
  "eventId": "770e8400-e29b-41d4-a716-446655440000"
}
```

#### Response (403 Forbidden)
```json
{
  "error": "premium_required",
  "message": "Spotify Premium subscription required for full track playback",
  "correlationId": "abc-123-def",
  "details": {
    "userSubscription": "free",
    "alternativeAction": "Use preview playback instead"
  }
}
```

#### Response (404 Not Found)
```json
{
  "error": "track_not_found",
  "message": "Track is not available or has been removed from Spotify",
  "correlationId": "abc-123-def",
  "details": {
    "trackId": "4VqPOruhp5EdPBeR92t6lQ"
  }
}
```

---

### POST /api/playback/play-preview
**Purpose**: Play 30-second preview of a track (available to all users)  
**Authentication**: Bearer Token  

#### Request
```json
{
  "trackId": "4VqPOruhp5EdPBeR92t6lQ",
  "sessionId": "660e8400-e29b-41d4-a716-446655440000",
  "deviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459"
}
```

#### Response (200 OK)
```json
{
  "playback": {
    "isPlaying": true,
    "isPreview": true,
    "previewDurationMs": 30000,
    "track": {
      "spotifyTrackId": "4VqPOruhp5EdPBeR92t6lQ",
      "name": "Don't Stop Believin'",
      "artistName": "Journey",
      "previewUrl": "https://p.scdn.co/mp3-preview/..."
    },
    "device": {
      "spotifyDeviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
      "name": "My Computer"
    },
    "timestamp": "2025-09-15T13:05:00Z"
  },
  "eventId": "770e8400-e29b-41d4-a716-446655440000"
}
```

#### Response (404 Not Found)
```json
{
  "error": "preview_unavailable",
  "message": "Preview is not available for this track",
  "correlationId": "abc-123-def",
  "details": {
    "trackId": "4VqPOruhp5EdPBeR92t6lQ",
    "reason": "No preview URL provided by Spotify"
  }
}
```

---

### POST /api/playback/pause
**Purpose**: Pause current playback on selected device  
**Authentication**: Bearer Token  

#### Request
```json
{
  "sessionId": "660e8400-e29b-41d4-a716-446655440000",
  "deviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459"
}
```

#### Response (200 OK)
```json
{
  "playback": {
    "isPlaying": false,
    "device": {
      "spotifyDeviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
      "name": "My Computer"
    },
    "timestamp": "2025-09-15T13:07:30Z"
  },
  "eventId": "880e8400-e29b-41d4-a716-446655440000"
}
```

#### Response (409 Conflict)
```json
{
  "error": "no_active_playback",
  "message": "No active playback to pause on the specified device",
  "correlationId": "abc-123-def"
}
```

---

### POST /api/playback/resume
**Purpose**: Resume paused playback on selected device  
**Authentication**: Bearer Token  

#### Request
```json
{
  "sessionId": "660e8400-e29b-41d4-a716-446655440000",
  "deviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459"
}
```

#### Response (200 OK)
```json
{
  "playback": {
    "isPlaying": true,
    "device": {
      "spotifyDeviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
      "name": "My Computer"
    },
    "timestamp": "2025-09-15T13:08:00Z"
  },
  "eventId": "990e8400-e29b-41d4-a716-446655440000"
}
```

---

### GET /api/playback/status
**Purpose**: Get current playback status for user's session  
**Authentication**: Bearer Token  

#### Request Query Parameters
- `sessionId`: Required, active session ID

#### Response (200 OK)
```json
{
  "playback": {
    "isPlaying": true,
    "track": {
      "spotifyTrackId": "4VqPOruhp5EdPBeR92t6lQ",
      "name": "Don't Stop Believin'",
      "artistName": "Journey",
      "albumName": "Escape",
      "durationMs": 251000
    },
    "device": {
      "spotifyDeviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
      "name": "My Computer",
      "volumePercent": 75
    },
    "progressMs": 45000,
    "isShuffled": false,
    "repeatState": "off",
    "timestamp": "2025-09-15T13:05:45Z"
  },
  "session": {
    "id": "660e8400-e29b-41d4-a716-446655440000",
    "currentQuestionIndex": 3,
    "status": "InProgress"
  }
}
```

#### Response (204 No Content)
No active playback for the session.

---

### POST /api/playback/transfer
**Purpose**: Transfer playback to a different device  
**Authentication**: Bearer Token  

#### Request
```json
{
  "targetDeviceId": "b4be4d64f9b19839f92426c71d8d0cd4e8f25a3e",
  "sessionId": "660e8400-e29b-41d4-a716-446655440000",
  "play": true
}
```

#### Request Validation
- `targetDeviceId`: Required, valid and available Spotify device ID
- `sessionId`: Required, active session ID owned by user
- `play`: Optional, start playback on target device, default false

#### Response (200 OK)
```json
{
  "transfer": {
    "success": true,
    "fromDevice": {
      "spotifyDeviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
      "name": "My Computer"
    },
    "toDevice": {
      "spotifyDeviceId": "b4be4d64f9b19839f92426c71d8d0cd4e8f25a3e",
      "name": "iPhone"
    },
    "timestamp": "2025-09-15T13:10:00Z"
  },
  "playback": {
    "isPlaying": true,
    "device": {
      "spotifyDeviceId": "b4be4d64f9b19839f92426c71d8d0cd4e8f25a3e",
      "name": "iPhone"
    }
  },
  "eventId": "aa0e8400-e29b-41d4-a716-446655440000"
}
```

---

### GET /api/playback/history/{sessionId}
**Purpose**: Get playback history for a quiz session  
**Authentication**: Bearer Token  

#### Request
Path parameter: `sessionId` (GUID)

#### Response (200 OK)
```json
{
  "session": {
    "id": "660e8400-e29b-41d4-a716-446655440000",
    "startedAt": "2025-09-15T13:00:00Z",
    "status": "InProgress"
  },
  "playbackEvents": [
    {
      "id": "770e8400-e29b-41d4-a716-446655440000",
      "action": "Play",
      "trackId": "4VqPOruhp5EdPBeR92t6lQ",
      "deviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
      "timestamp": "2025-09-15T13:05:00Z",
      "success": true,
      "position": 0
    },
    {
      "id": "880e8400-e29b-41d4-a716-446655440000",
      "action": "Pause",
      "deviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
      "timestamp": "2025-09-15T13:07:30Z",
      "success": true,
      "position": 150000
    }
  ],
  "summary": {
    "totalEvents": 2,
    "playEvents": 1,
    "pauseEvents": 1,
    "transferEvents": 0,
    "failedEvents": 0
  }
}
```

## Device Requirements & Compatibility

### Spotify Premium Features
- **Full Track Playback**: Requires Spotify Premium subscription
- **Device Transfer**: Available to all users
- **Volume Control**: Premium users only
- **Seek/Position**: Premium users only

### Free Account Features
- **Preview Playback**: 30-second clips available to all users
- **Device Listing**: Available to all users
- **Basic Controls**: Play/Pause previews available to all users

### Device Types & Restrictions
- **Computer**: Full playback control available
- **Smartphone**: Full playback control available
- **Speaker/TV**: May have restrictions based on device capabilities
- **Web Player**: Limited to browser-based controls

## Rate Limiting & Performance

### Rate Limits per User
- **Device Operations**: 60 per minute
- **Playback Controls**: 120 per minute (2 per second)
- **Status Checks**: 300 per minute

### Performance Targets
- **Device Listing**: <1 second response time
- **Playback Control**: <2 seconds for play/pause commands
- **Device Transfer**: <3 seconds for transfer completion

### Error Recovery
- **Device Unavailable**: Automatic retry with exponential backoff
- **Network Issues**: Queue commands for retry when connection restored
- **Spotify API Limits**: Respect rate limits and provide user feedback

## Security & Privacy

### Data Protection
- **Device Information**: Cached for session duration only
- **Playback History**: Retained for 7 days for debugging
- **User Preferences**: Stored securely with encryption

### Access Control
- **Session Validation**: All requests validated against active user sessions
- **Device Authorization**: Only user's own devices accessible
- **Cross-User Protection**: No access to other users' playback state