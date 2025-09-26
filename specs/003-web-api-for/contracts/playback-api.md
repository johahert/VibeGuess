# API Playback Control Endpoints

**Base URL**: `/api/playback`  
**Version**: v1  
**Authentication**: Bearer Token (required)  
**Requirements**: Authenticated Spotify user with valid access token

## Endpoints Overview

### GET /api/playback/devices
**Purpose**: Get user's available Spotify devices (real-time from Spotify API)  
**Authentication**: Bearer Token with Spotify user authentication required  

#### Request
Query Parameters:
- `includeRestricted`: boolean (optional) - Include restricted devices in response, default true

#### Response (200 OK)
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
    },
    {
      "id": "b4be4d64f9b19839f92426c71d8d0cd4e8f25a3e",
      "name": "iPhone",
      "type": "Smartphone",
      "isActive": false,
      "isPrivateSession": false,
      "isRestricted": false,
      "volumePercent": 50,
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
  }
}
```

#### Response (401 Unauthorized)
```json
{
  "error": "authentication_required",
  "message": "User must be authenticated with Spotify to access devices",
  "correlationId": "abc-123-def"
}
```

#### Response (503 Service Unavailable)
```json
{
  "error": "service_unavailable",
  "message": "Spotify service is temporarily unavailable",
  "correlationId": "abc-123-def"
}
```

---

### GET /api/playback/status
**Purpose**: Get current playback status from Spotify API (real-time)  
**Authentication**: Bearer Token with Spotify user authentication required  

#### Request Query Parameters
- `includeContext`: boolean (optional) - Include playlist/album context information, default false
- `market`: string (optional) - Market code for track relinking (e.g., "US"), default user's market

#### Response (200 OK) - Active Playback
```json
{
  "isPlaying": true,
  "timestamp": 1727356800000,
  "progressMs": 60000,
  "shuffleState": false,
  "repeatState": "off",
  "item": {
    "id": "4iV5W9uYEdYUVa79Axb7Rh",
    "name": "Bohemian Rhapsody",
    "type": "track",
    "uri": "spotify:track:4iV5W9uYEdYUVa79Axb7Rh",
    "durationMs": 355000,
    "artists": [
      {
        "id": "1dfeR4HaWDbWqFHLkxsg1d",
        "name": "Queen"
      }
    ],
    "album": {
      "id": "6i6folBtxKV28WX3msQ4FE",
      "name": "A Night at the Opera",
      "images": [
        {
          "url": "https://i.scdn.co/image/album.jpg",
          "height": 640,
          "width": 640
        }
      ]
    }
  },
  "device": {
    "id": "ed01a3ca8def0a1772eab7be6c4b0bb37b06163e",
    "name": "My Computer",
    "type": "Computer",
    "volumePercent": 75,
    "isActive": true
  },
  "actions": {
    "interrupting_playback": true,
    "pausing": true,
    "resuming": true,
    "seeking": true,
    "skipping_next": true,
    "skipping_prev": true,
    "toggling_repeat_context": true,
    "toggling_shuffle": true,
    "transferring_playback": true,
    "disallows": {
      "resuming": false,
      "skipping_next": false,
      "skipping_prev": false
    }
  }
}
```

#### Response (200 OK) - No Active Playback
```json
{
  "isPlaying": false,
  "timestamp": 1727356800000,
  "progressMs": null,
  "item": null,
  "device": null,
  "shuffleState": false,
  "repeatState": "off"
}
```

---

### POST /api/playback/play
**Purpose**: Start or resume playback on user's Spotify device (real Spotify API integration)  
**Authentication**: Bearer Token with Spotify user authentication and Premium subscription required  

#### Request
```json
{
  "deviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
  "uris": ["spotify:track:4VqPOruhp5EdPBeR92t6lQ", "spotify:track:1A8XwHp7jKEYgqp7eAtD8J"],
  "trackUri": "spotify:track:4VqPOruhp5EdPBeR92t6lQ",
  "contextUri": "spotify:playlist:37i9dQZF1DXcBWIGoYBM5M",
  "positionMs": 0,
  "volume": 75
}
```

#### Request Fields
- `deviceId`: string (optional) - Target device ID, uses active device if not provided
- `uris`: string[] (optional) - Array of track URIs to play
- `trackUri`: string (optional) - Single track URI to play (alternative to uris)
- `contextUri`: string (optional) - Context URI (playlist, album) to play from
- `positionMs`: integer (optional) - Starting position in milliseconds, default 0
- `volume`: integer (optional) - Playback volume 0-100

#### Response (200 OK)
```json
{
  "isPlaying": true,
  "trackUri": "spotify:track:4VqPOruhp5EdPBeR92t6lQ",
  "deviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
  "positionMs": 0,
  "progressMs": 0,
  "timestamp": 1727356800000,
  "item": {
    "id": "4VqPOruhp5EdPBeR92t6lQ",
    "name": "Don't Stop Believin'",
    "uri": "spotify:track:4VqPOruhp5EdPBeR92t6lQ",
    "durationMs": 251000
  },
  "device": {
    "id": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
    "name": "My Computer",
    "type": "Computer",
    "volumePercent": 75
  }
}
```

#### Response (401 Unauthorized)
```json
{
  "error": "authentication_required",
  "message": "User must be authenticated with Spotify to control playback",
  "correlationId": "abc-123-def"
}
```

#### Response (403 Forbidden)
```json
{
  "error": "premium_required",
  "message": "Spotify Premium subscription required for playback control",
  "correlationId": "abc-123-def"
}
```

#### Response (404 Not Found)
```json
{
  "error": "no_active_device",
  "message": "No active device found. Please start Spotify on a device first.",
  "correlationId": "abc-123-def"
}
```

---

### POST /api/playback/pause
**Purpose**: Pause current playback on user's Spotify device (real Spotify API integration)  
**Authentication**: Bearer Token with Spotify user authentication and Premium subscription required  

#### Request
```json
{
  "deviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459"
}
```

#### Request Fields
- `deviceId`: string (optional) - Target device ID, uses active device if not provided

#### Response (200 OK)
```json
{
  "isPlaying": false,
  "deviceId": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
  "progressMs": 120000,
  "pausedAt": 1727356920000,
  "timestamp": 1727356920000,
  "item": {
    "id": "4VqPOruhp5EdPBeR92t6lQ",
    "name": "Don't Stop Believin'",
    "uri": "spotify:track:4VqPOruhp5EdPBeR92t6lQ",
    "durationMs": 251000
  },
  "device": {
    "id": "ed01a3fd8cd1a2a5654e549e75e14d8697984459",
    "name": "My Computer",
    "type": "Computer",
    "volumePercent": 75
  }
}
```

#### Response (401 Unauthorized)
```json
{
  "error": "authentication_required",
  "message": "User must be authenticated with Spotify to control playback",
  "correlationId": "abc-123-def"
}
```

#### Response (403 Forbidden)
```json
{
  "error": "premium_required",
  "message": "Spotify Premium subscription required for playback control",
  "correlationId": "abc-123-def"
}
```

#### Response (404 Not Found)
```json
{
  "error": "no_active_device",
  "message": "No active device found. Please start Spotify on a device first.",
  "correlationId": "abc-123-def"
}
```

#### Response (503 Service Unavailable)
```json
{
  "error": "service_unavailable",
  "message": "Failed to pause playback on Spotify",
  "correlationId": "abc-123-def"
}
```

## Error Responses

All endpoints may return the following error responses:

#### Response (401 Unauthorized)
```json
{
  "error": "authentication_required",
  "message": "User must be authenticated with Spotify to control playback",
  "correlationId": "abc-123-def"
}
```

#### Response (503 Service Unavailable)
```json
{
  "error": "service_unavailable", 
  "message": "Spotify service is temporarily unavailable",
  "correlationId": "abc-123-def"
}

## Authentication & Requirements

### User Authentication
- **Spotify User Login**: Required for all playback operations
- **Access Token**: Must have valid Spotify user access token (not client credentials)
- **Scopes Required**: `user-read-playback-state`, `user-modify-playback-state`, `user-read-currently-playing`

### Spotify Premium Requirements
- **Playback Control**: Requires Spotify Premium subscription for play/pause operations
- **Device Listing**: Available to all authenticated users (Free and Premium)
- **Playback Status**: Available to all authenticated users (Free and Premium)

### Device Requirements
- **Active Device**: User must have Spotify running on at least one device for playback control
- **Device Types**: All Spotify Connect-enabled devices supported (Computer, Smartphone, Speaker, TV, etc.)
- **Network**: Devices must be connected to the internet and available on Spotify Connect

## Real-time Integration

### Spotify Web API Integration
- **Live Data**: All responses return real-time data from Spotify's servers
- **No Caching**: Device and playback status are fetched fresh on each request
- **API Rate Limits**: Respects Spotify Web API rate limits and quotas

### Performance Characteristics
- **Device Listing**: Typically 200-500ms response time
- **Playback Control**: Typically 300-800ms for play/pause commands
- **Status Retrieval**: Typically 150-400ms response time
- **Network Dependent**: Performance varies based on Spotify API response times

### Error Handling
- **Spotify API Errors**: Translated to appropriate HTTP status codes
- **Network Timeouts**: Graceful handling with proper error messages
- **Authentication Issues**: Clear error messages for token problems
- **Premium Requirements**: Specific error codes for subscription-related restrictions

## Security & Privacy

### Data Handling
- **No Data Storage**: Playback data is not cached or stored locally
- **Real-time Only**: All data comes directly from Spotify's servers
- **User Isolation**: Each user can only access their own devices and playback state

### Access Control
- **JWT Authentication**: All requests require valid Bearer token
- **Spotify Token Validation**: User's Spotify access token validated on each request
- **Scope Verification**: Ensures user has granted necessary permissions