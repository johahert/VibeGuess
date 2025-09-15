# API Health & Testing Endpoints

**Base URL**: `/api/health`  
**Version**: v1  
**Authentication**: Health checks (None), Test endpoints (Bearer Token)

## Health Check Endpoints

### GET /api/health
**Purpose**: Basic application health check  
**Authentication**: None  

#### Request
No request body or parameters required.

#### Response (200 OK)
```json
{
  "status": "Healthy",
  "timestamp": "2025-09-15T13:15:00Z",
  "version": "1.0.0",
  "environment": "Production"
}
```

#### Response (503 Service Unavailable)
```json
{
  "status": "Unhealthy",
  "timestamp": "2025-09-15T13:15:00Z",
  "version": "1.0.0",
  "environment": "Production",
  "errors": [
    "Database connection failed",
    "Spotify API unavailable"
  ]
}
```

---

### GET /api/health/detailed
**Purpose**: Detailed health check with dependency status  
**Authentication**: None (but may be restricted by infrastructure)  

#### Response (200 OK)
```json
{
  "status": "Healthy",
  "timestamp": "2025-09-15T13:15:00Z",
  "version": "1.0.0",
  "environment": "Production",
  "dependencies": {
    "database": {
      "status": "Healthy",
      "responseTime": "15ms",
      "details": "SQL Server connection active"
    },
    "spotify": {
      "status": "Healthy", 
      "responseTime": "250ms",
      "details": "Spotify Web API responding normally"
    },
    "openai": {
      "status": "Healthy",
      "responseTime": "1200ms",
      "details": "OpenAI API responding normally"
    },
    "redis": {
      "status": "Healthy",
      "responseTime": "5ms",
      "details": "Redis cache connection active"
    }
  },
  "performance": {
    "cpuUsage": "25%",
    "memoryUsage": "512MB",
    "activeConnections": 45,
    "requestsPerMinute": 120
  }
}
```

#### Response (503 Service Unavailable)
```json
{
  "status": "Unhealthy",
  "timestamp": "2025-09-15T13:15:00Z",
  "version": "1.0.0",
  "environment": "Production",
  "dependencies": {
    "database": {
      "status": "Healthy",
      "responseTime": "15ms"
    },
    "spotify": {
      "status": "Unhealthy",
      "responseTime": "timeout",
      "error": "Connection timeout after 30 seconds"
    },
    "openai": {
      "status": "Degraded",
      "responseTime": "5000ms",
      "warning": "Response time above threshold"
    },
    "redis": {
      "status": "Healthy",
      "responseTime": "5ms"
    }
  }
}
```

---

## Testing Endpoints

### POST /api/health/test/auth
**Purpose**: Test Spotify authentication flow without full OAuth  
**Authentication**: Bearer Token  

#### Request
```json
{
  "testUserId": "test-spotify-user-123"
}
```

#### Response (200 OK)
```json
{
  "test": "auth",
  "success": true,
  "timestamp": "2025-09-15T13:15:00Z",
  "results": {
    "tokenValidation": "passed",
    "spotifyConnection": "passed",
    "userProfileRetrieval": "passed",
    "tokenRefresh": "passed"
  },
  "execution": {
    "durationMs": 1250,
    "steps": 4,
    "warnings": []
  }
}
```

#### Response (400 Bad Request)
```json
{
  "test": "auth",
  "success": false,
  "timestamp": "2025-09-15T13:15:00Z",
  "error": "Authentication test failed",
  "results": {
    "tokenValidation": "passed",
    "spotifyConnection": "failed",
    "userProfileRetrieval": "skipped",
    "tokenRefresh": "skipped"
  },
  "details": {
    "failedStep": "spotifyConnection",
    "errorMessage": "Unable to connect to Spotify API",
    "suggestions": ["Check Spotify API credentials", "Verify network connectivity"]
  }
}
```

---

### POST /api/health/test/quiz-generation
**Purpose**: Test quiz generation with mock data  
**Authentication**: Bearer Token  

#### Request
```json
{
  "testPrompt": "Test quiz about rock music",
  "mockMode": true,
  "validateTracks": false
}
```

#### Response (200 OK)
```json
{
  "test": "quiz-generation",
  "success": true,
  "timestamp": "2025-09-15T13:15:00Z",
  "results": {
    "promptProcessing": "passed",
    "aiConnection": "passed", 
    "contentGeneration": "passed",
    "trackValidation": "skipped",
    "quizAssembly": "passed"
  },
  "generatedQuiz": {
    "id": "test-quiz-550e8400-e29b-41d4-a716-446655440000",
    "questionCount": 5,
    "format": "MultipleChoice",
    "hasAudioTracks": false
  },
  "execution": {
    "durationMs": 3200,
    "steps": 4,
    "aiTokensUsed": 1250
  }
}
```

---

### POST /api/health/test/playback
**Purpose**: Test Spotify playback integration  
**Authentication**: Bearer Token  

#### Request
```json
{
  "testTrackId": "4VqPOruhp5EdPBeR92t6lQ",
  "mockDevice": true,
  "testPreviewOnly": true
}
```

#### Response (200 OK)
```json
{
  "test": "playback",
  "success": true,
  "timestamp": "2025-09-15T13:15:00Z",
  "results": {
    "deviceListing": "passed",
    "deviceSelection": "passed",
    "trackValidation": "passed",
    "playbackControl": "passed",
    "pauseControl": "passed"
  },
  "testDetails": {
    "devicesFound": 2,
    "trackPlayable": true,
    "previewAvailable": true,
    "controlsResponsive": true
  },
  "execution": {
    "durationMs": 2100,
    "steps": 5,
    "warnings": ["Using mock device for testing"]
  }
}
```

---

### POST /api/health/test/end-to-end
**Purpose**: Complete end-to-end workflow test  
**Authentication**: Bearer Token  

#### Request
```json
{
  "testScenario": "complete-quiz-with-playback",
  "useMockData": true,
  "skipExternalAPIs": false
}
```

#### Response (200 OK)
```json
{
  "test": "end-to-end",
  "success": true,
  "timestamp": "2025-09-15T13:15:00Z",
  "scenario": "complete-quiz-with-playback",
  "results": {
    "userAuthentication": "passed",
    "quizGeneration": "passed",
    "sessionCreation": "passed",
    "deviceSelection": "passed", 
    "playbackControl": "passed",
    "answerSubmission": "passed",
    "sessionCompletion": "passed"
  },
  "workflow": {
    "totalSteps": 7,
    "completedSteps": 7,
    "totalDuration": "12.5s",
    "questionsAnswered": 5,
    "tracksPlayed": 3,
    "score": "4/5"
  },
  "performance": {
    "quizGenerationTime": "4.2s",
    "averagePlaybackLatency": "1.8s",
    "sessionResponseTime": "250ms"
  }
}
```

#### Response (500 Internal Server Error)
```json
{
  "test": "end-to-end", 
  "success": false,
  "timestamp": "2025-09-15T13:15:00Z",
  "scenario": "complete-quiz-with-playback",
  "results": {
    "userAuthentication": "passed",
    "quizGeneration": "passed",
    "sessionCreation": "failed",
    "deviceSelection": "skipped",
    "playbackControl": "skipped",
    "answerSubmission": "skipped",
    "sessionCompletion": "skipped"
  },
  "error": {
    "failedStep": "sessionCreation",
    "errorMessage": "Unable to create quiz session - database constraint violation",
    "errorCode": "DB_CONSTRAINT_ERROR",
    "suggestions": [
      "Check database schema integrity",
      "Verify user data consistency",
      "Review session creation logic"
    ]
  }
}
```

---

### GET /api/health/test/mock-data
**Purpose**: Get available mock data for testing  
**Authentication**: Bearer Token  

#### Response (200 OK)
```json
{
  "mockData": {
    "users": [
      {
        "id": "test-user-1",
        "displayName": "Test User Premium",
        "hasSpotifyPremium": true,
        "country": "US"
      },
      {
        "id": "test-user-2", 
        "displayName": "Test User Free",
        "hasSpotifyPremium": false,
        "country": "US"
      }
    ],
    "tracks": [
      {
        "spotifyTrackId": "4VqPOruhp5EdPBeR92t6lQ",
        "name": "Don't Stop Believin'",
        "artistName": "Journey",
        "hasPreview": true,
        "isPlayable": true
      },
      {
        "spotifyTrackId": "7qiZfU4dY1lWllzX7mPBI3",
        "name": "Sweet Child O' Mine", 
        "artistName": "Guns N' Roses",
        "hasPreview": false,
        "isPlayable": true
      }
    ],
    "devices": [
      {
        "spotifyDeviceId": "mock-device-computer",
        "name": "Test Computer",
        "type": "Computer",
        "isRestricted": false
      },
      {
        "spotifyDeviceId": "mock-device-phone",
        "name": "Test Phone",
        "type": "Smartphone", 
        "isRestricted": false
      }
    ],
    "quizPrompts": [
      "Create a quiz about 80s rock music",
      "Test quiz about pop artists from the 2000s",
      "Generate questions about classical music composers"
    ]
  }
}
```

---

## Monitoring & Metrics Endpoints

### GET /api/health/metrics
**Purpose**: Application performance metrics  
**Authentication**: None (may be restricted by infrastructure)  

#### Response (200 OK)
```json
{
  "timestamp": "2025-09-15T13:15:00Z",
  "metrics": {
    "requests": {
      "total": 15420,
      "perMinute": 125,
      "errorRate": "0.5%"
    },
    "quiz": {
      "generated": 1250,
      "averageGenerationTime": "4.1s",
      "successRate": "98.2%"
    },
    "playback": {
      "totalCommands": 5680,
      "averageLatency": "1.7s",
      "successRate": "99.1%"
    },
    "authentication": {
      "activeUsers": 89,
      "tokenRefreshes": 234,
      "failedLogins": 12
    },
    "external": {
      "spotifyApiCalls": 8920,
      "spotifyErrorRate": "1.2%",
      "openaiApiCalls": 1250,
      "openaiErrorRate": "0.8%"
    }
  }
}
```

## Configuration & Environment

### Environment Variables
```bash
# Application
ASPNETCORE_ENVIRONMENT=Production
VIBEGUESS_VERSION=1.0.0

# Health Check Configuration  
HEALTH_CHECK_TIMEOUT_SECONDS=30
HEALTH_CHECK_CACHE_SECONDS=60

# Test Configuration
ENABLE_TEST_ENDPOINTS=true
MOCK_DATA_ENABLED=true
TEST_USER_PREFIX=test-

# External API Timeouts
SPOTIFY_API_TIMEOUT_SECONDS=30
OPENAI_API_TIMEOUT_SECONDS=60
```

### Health Check Intervals
- **Basic Health**: Every 30 seconds
- **Dependency Health**: Every 60 seconds  
- **Performance Metrics**: Every 5 minutes
- **External API Status**: Every 2 minutes