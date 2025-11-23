# VibeGuess SignalR WebSocket Protocol Specification

## Overview

This document describes the SignalR WebSocket protocol for real-time communication in VibeGuess live quiz sessions. The protocol uses Microsoft SignalR for reliable bidirectional communication between hosts, participants, and the server.

## Connection Information

- **Hub URL**: `/hubs/hostedquiz`
- **Protocol**: SignalR with JSON Hub Protocol
- **Transport**: WebSockets (preferred), Server-Sent Events (fallback)
- **Authentication**: None (currently) - JWT Bearer in production

## Client Libraries

### JavaScript/TypeScript
```bash
npm install @microsoft/signalr
```

### C# 
```bash
dotnet add package Microsoft.AspNetCore.SignalR.Client
```

### Other Languages
- Java: [SignalR Java Client](https://github.com/SignalR/java-client)
- Python: [signalrcore](https://pypi.org/project/signalrcore/)
- Swift: [SignalRSwift](https://github.com/moozzyk/SignalR-Client-Swift)

## Connection Management

### Establishing Connection

```javascript
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

const connection = new HubConnectionBuilder()
  .withUrl('/hubs/hostedquiz', {
    transport: HttpTransportType.WebSockets | HttpTransportType.ServerSentEvents
  })
  .configureLogging(LogLevel.Information)
  .withAutomaticReconnect()
  .build();

await connection.start();
```

### Connection States
- **Disconnected**: Initial state, not connected
- **Connecting**: Attempting to establish connection
- **Connected**: Successfully connected and ready
- **Reconnecting**: Attempting to reconnect after disconnection

### Automatic Reconnection
```javascript
connection.onreconnecting(error => {
  console.log(`Connection lost due to error "${error}". Reconnecting.`);
});

connection.onreconnected(connectionId => {
  console.log(`Connection reestablished. Connected with connectionId "${connectionId}".`);
});

connection.onclose(error => {
  console.log(`Connection closed due to error "${error}". Try refreshing this page.`);
});
```

## Message Format

All messages follow the SignalR JSON Hub Protocol format:

```json
{
  "type": 1,           // Message type (1 = invocation, 3 = completion, etc.)
  "target": "method",  // Method name for server-to-client calls
  "arguments": [],     // Method arguments
  "invocationId": "1"  // Optional invocation ID for tracking
}
```

## Host Methods (Client-to-Server)

### 1. CreateSession

Creates a new live quiz session.

**Method**: `CreateSession`

**Parameters**:
1. `quizId` (string): Unique identifier of the quiz to host
2. `title` (string): Human-readable session title

**Request Example**:
```javascript
const result = await connection.invoke('CreateSession', 'quiz-123', 'Friday Music Quiz');
```

**Response Schema**:
```typescript
{
  success: boolean;
  sessionId?: string;    // UUID format
  joinCode?: string;     // 6-character alphanumeric
  error?: string;        // Error message if success = false
}
```

**Success Response Example**:
```json
{
  "success": true,
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "joinCode": "ABC123"
}
```

**Error Response Example**:
```json
{
  "success": false,
  "error": "Failed to create session"
}
```

---

### 2. StartGame

Transitions session from Lobby to Active state.

**Method**: `StartGame`

**Parameters**:
1. `sessionId` (string): Session UUID

**Authorization**: Must be called by session host

**Side Effects**: Broadcasts `GameStarted` event to all participants

**Request Example**:
```javascript
const result = await connection.invoke('StartGame', 'session-550e8400-e29b-41d4-a716-446655440000');
```

**Response Schema**:
```typescript
{
  success: boolean;
  error?: string;
}
```

---

### 3. NextQuestion

Advances to the next question in the quiz.

**Method**: `NextQuestion`

**Parameters**:
1. `sessionId` (string): Session UUID
2. `questionIndex` (number): Zero-based question index
3. `questionData` (object): Question information

**Question Data Schema**:
```typescript
{
  questionId: string;
  questionText: string;
  options: string[];           // Array of answer choices
  correctAnswer: string;       // Correct answer (host only)
  timeLimit?: number;          // Override default time limit
  type: "multiple-choice" | "true-false" | "text";
  metadata?: {                 // Optional additional data
    difficulty?: number;       // 1-5 scale
    category?: string;
    points?: number;           // Override default points
  }
}
```

**Authorization**: Must be called by session host

**Side Effects**: 
- Broadcasts `NewQuestion` event to participants (without correct answer)
- Resets participant answer states for current question
- Starts question timer

**Request Example**:
```javascript
const questionData = {
  questionId: "q1",
  questionText: "Which artist sang 'Bohemian Rhapsody'?",
  options: ["Queen", "The Beatles", "Led Zeppelin", "Pink Floyd"],
  correctAnswer: "Queen",
  type: "multiple-choice",
  metadata: {
    difficulty: 3,
    category: "Classic Rock"
  }
};

const result = await connection.invoke('NextQuestion', sessionId, 1, questionData);
```

**Response Schema**:
```typescript
{
  success: boolean;
  error?: string;
}
```

---

### 4. EndSession

Ends the quiz session and shows final results.

**Method**: `EndSession`

**Parameters**:
1. `sessionId` (string): Session UUID

**Authorization**: Must be called by session host

**Side Effects**: 
- Changes session state to Completed
- Broadcasts `GameEnded` event with final leaderboard
- Triggers optional analytics persistence

**Request Example**:
```javascript
const result = await connection.invoke('EndSession', sessionId);
```

**Response Schema**:
```typescript
{
  success: boolean;
  error?: string;
}
```

---

### 5. RemovePlayer

Removes a participant from the session (moderation).

**Method**: `RemovePlayer`

**Parameters**:
1. `sessionId` (string): Session UUID
2. `participantId` (string): Participant UUID to remove

**Authorization**: Must be called by session host

**Side Effects**: 
- Removes participant from session
- Sends `RemovedFromSession` to removed participant
- Broadcasts `ParticipantLeft` to remaining participants

**Request Example**:
```javascript
const result = await connection.invoke('RemovePlayer', sessionId, 'participant-123');
```

**Response Schema**:
```typescript
{
  success: boolean;
  error?: string;
}
```

## Player Methods (Client-to-Server)

### 1. JoinSession

Joins a live quiz session using join code.

**Method**: `JoinSession`

**Parameters**:
1. `joinCode` (string): 6-character session join code
2. `displayName` (string): Participant's display name (must be unique in session)

**Business Rules**:
- Display name must be unique within session
- Session must be in Lobby or Active state
- Maximum participants limit (configurable, default: 50)

**Side Effects**: 
- Adds participant to session groups
- Broadcasts `ParticipantJoined` to host and other participants

**Request Example**:
```javascript
const result = await connection.invoke('JoinSession', 'ABC123', 'Alice');
```

**Response Schema**:
```typescript
{
  success: boolean;
  sessionId?: string;
  participantId?: string;       // UUID for tracking participant
  sessionState?: "Lobby" | "Active" | "Paused" | "Completed";
  currentQuestionIndex?: number;
  error?: string;
}
```

**Success Response Example**:
```json
{
  "success": true,
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "participantId": "participant-789",
  "sessionState": "Lobby",
  "currentQuestionIndex": 0
}
```

**Error Cases**:
- `"Session not found"`: Invalid join code
- `"Display name is already taken"`: Name collision
- `"Session is not accepting new participants"`: Game ended or paused

---

### 2. SubmitAnswer

Submits an answer to the current question.

**Method**: `SubmitAnswer`

**Parameters**:
1. `sessionId` (string): Session UUID
2. `participantId` (string): Participant UUID
3. `selectedAnswer` (string): Chosen answer

**Business Rules**:
- Can only answer once per question
- Must be called during active question period
- Answer must match one of the provided options (for multiple choice)

**Scoring Algorithm**:
- Base score: 100 points for correct answer
- Time bonus: 0-50 points based on response speed
- Total score = Base + Time Bonus (if correct), 0 (if incorrect)

**Side Effects**: 
- Updates participant score and statistics
- Sends `AnswerSubmitted` to host
- Broadcasts `LeaderboardUpdate` to all participants

**Request Example**:
```javascript
const result = await connection.invoke('SubmitAnswer', sessionId, participantId, 'Queen');
```

**Response Schema**:
```typescript
{
  success: boolean;
  isCorrect?: boolean;          // Whether answer was correct
  score?: number;               // Points earned for this answer
  totalScore?: number;          // Participant's total accumulated score
  timeBonus?: number;           // Time bonus points earned
  responseTime?: string;        // ISO 8601 duration
  error?: string;
}
```

**Success Response Example**:
```json
{
  "success": true,
  "isCorrect": true,
  "score": 135,
  "totalScore": 450,
  "timeBonus": 35,
  "responseTime": "PT5.2S"
}
```

---

### 3. LeaveSession

Leaves the current session.

**Method**: `LeaveSession`

**Parameters**:
1. `sessionId` (string): Session UUID
2. `participantId` (string): Participant UUID

**Authorization**: Must be called by the participant themselves

**Side Effects**: 
- Removes participant from session groups
- Broadcasts `ParticipantLeft` to remaining participants

**Request Example**:
```javascript
const result = await connection.invoke('LeaveSession', sessionId, participantId);
```

**Response Schema**:
```typescript
{
  success: boolean;
  error?: string;
}
```

## Server-to-Client Events

### 1. GameStarted

Sent when host starts the game.

**Event**: `GameStarted`

**Recipients**: All participants in session

**Payload Schema**:
```typescript
{
  sessionId: string;
  message?: string;            // Optional start message
}
```

**Handler Example**:
```javascript
connection.on('GameStarted', (data) => {
  console.log(`Game started for session: ${data.sessionId}`);
  // Transition UI to game mode
  showGameInterface();
});
```

---

### 2. NewQuestion

Sent when host advances to new question.

**Event**: `NewQuestion`

**Recipients**: All participants in session

**Payload Schema**:
```typescript
{
  sessionId: string;
  questionIndex: number;
  question: {
    questionId: string;
    questionText: string;
    options: string[];
    type: "multiple-choice" | "true-false" | "text";
    metadata?: {
      difficulty?: number;
      category?: string;
    }
    // Note: correctAnswer is NOT included for participants
  };
  timeLimit: number;           // Seconds for this question
  startTime: string;           // ISO 8601 timestamp
}
```

**Handler Example**:
```javascript
connection.on('NewQuestion', (data) => {
  displayQuestion(data.question);
  startTimer(data.timeLimit);
  enableAnswerSubmission();
});
```

---

### 3. GameEnded

Sent when host ends the session.

**Event**: `GameEnded`

**Recipients**: All participants in session

**Payload Schema**:
```typescript
{
  sessionId: string;
  leaderboard: Array<{
    displayName: string;
    score: number;
    correctAnswers: number;
    totalAnswers: number;
    accuracy: number;          // Percentage
    rank: number;              // 1-based ranking
  }>;                          // Top 10 participants
  sessionStats: {
    totalParticipants: number;
    totalQuestions: number;
    averageScore: number;
    gameurationDuration: string;  // ISO 8601 duration
  };
}
```

**Handler Example**:
```javascript
connection.on('GameEnded', (data) => {
  showFinalResults(data.leaderboard, data.sessionStats);
  disableGameControls();
});
```

---

### 4. ParticipantJoined

Sent when new participant joins session.

**Event**: `ParticipantJoined`

**Recipients**: Host and existing participants (not the newly joined participant)

**Payload Schema**:
```typescript
{
  sessionId: string;
  participant: {
    participantId: string;
    displayName: string;
    joinedAt: string;          // ISO 8601 timestamp
  };
  participantCount: number;    // Updated total count
}
```

**Handler Example**:
```javascript
connection.on('ParticipantJoined', (data) => {
  addParticipantToList(data.participant);
  updateParticipantCount(data.participantCount);
  showNotification(`${data.participant.displayName} joined the game`);
});
```

---

### 5. ParticipantLeft

Sent when participant leaves or is removed.

**Event**: `ParticipantLeft`

**Recipients**: All remaining participants and host

**Payload Schema**:
```typescript
{
  sessionId: string;
  participantId: string;
  displayName?: string;        // May not be available if participant already removed
  participantCount: number;    // Updated total count
  reason?: "left" | "removed" | "disconnected";
}
```

**Handler Example**:
```javascript
connection.on('ParticipantLeft', (data) => {
  removeParticipantFromList(data.participantId);
  updateParticipantCount(data.participantCount);
  if (data.reason === 'removed') {
    showNotification(`${data.displayName} was removed by host`);
  }
});
```

---

### 6. AnswerSubmitted

Sent to host when participant submits answer.

**Event**: `AnswerSubmitted`

**Recipients**: Host only

**Payload Schema**:
```typescript
{
  sessionId: string;
  participantId: string;
  participantName: string;
  questionIndex: number;
  hasAnswered: boolean;        // Always true for this event
  submittedAt: string;         // ISO 8601 timestamp
  responseTime: string;        // ISO 8601 duration from question start
}
```

**Handler Example**:
```javascript
connection.on('AnswerSubmitted', (data) => {
  markParticipantAnswered(data.participantId);
  updateAnswerProgress();
  logAnswer(data.participantName, data.responseTime);
});
```

---

### 7. LeaderboardUpdate

Sent when scores are updated after answers.

**Event**: `LeaderboardUpdate`

**Recipients**: All participants in session

**Payload Schema**:
```typescript
{
  sessionId: string;
  leaderboard: Array<{
    participantId: string;
    displayName: string;
    score: number;
    correctAnswers: number;
    totalAnswers: number;
    rank: number;              // 1-based ranking
    scoreChange?: number;      // Points gained in last answer (+/- or 0)
  }>;                          // Top 10 participants
  lastUpdated: string;         // ISO 8601 timestamp
}
```

**Handler Example**:
```javascript
connection.on('LeaderboardUpdate', (data) => {
  updateLeaderboardDisplay(data.leaderboard);
  highlightScoreChanges(data.leaderboard);
  animateRankingChanges();
});
```

---

### 8. RemovedFromSession

Sent to participant when removed by host.

**Event**: `RemovedFromSession`

**Recipients**: Removed participant only

**Payload Schema**:
```typescript
{
  sessionId: string;
  reason: string;              // Human-readable reason
  canRejoin: boolean;          // Whether participant can rejoin
}
```

**Handler Example**:
```javascript
connection.on('RemovedFromSession', (data) => {
  showRemovalMessage(data.reason);
  if (!data.canRejoin) {
    redirectToHomePage();
  } else {
    showRejoinOption();
  }
});
```

## Groups and Targeting

SignalR uses groups to manage message routing:

### Group Names
- `session_{sessionId}`: All participants in a session
- `host_{sessionId}`: Host-only group for each session

### Message Targeting
```csharp
// Send to all participants in session
await Clients.Group($"session_{sessionId}").SendAsync("NewQuestion", questionData);

// Send to host only
await Clients.Group($"host_{sessionId}").SendAsync("AnswerSubmitted", answerData);

// Send to specific participant
await Clients.Client(connectionId).SendAsync("RemovedFromSession", removalData);

// Send to all except sender
await Clients.GroupExcept($"session_{sessionId}", Context.ConnectionId)
    .SendAsync("ParticipantJoined", joinData);
```

## Error Handling

### Connection Errors
```javascript
connection.onclose(async (error) => {
  if (error) {
    console.error('Connection closed due to error:', error);
  }
  
  // Attempt reconnection
  try {
    await connection.start();
    // Rejoin session if applicable
    if (sessionState.participantId && sessionState.sessionId) {
      // Note: May need special rejoining logic
    }
  } catch (reconnectError) {
    console.error('Reconnection failed:', reconnectError);
  }
});
```

### Method Call Errors
```javascript
async function safeInvoke(methodName, ...args) {
  try {
    const result = await connection.invoke(methodName, ...args);
    if (result && !result.success) {
      handleBusinessError(result.error);
      return null;
    }
    return result;
  } catch (error) {
    handleNetworkError(error);
    return null;
  }
}
```

### Business Logic Errors
All hub methods return objects with `success` boolean and optional `error` string for consistent error handling.

## Performance Considerations

### Connection Limits
- Default: 100 concurrent connections per server
- Consider connection pooling for load testing
- Monitor connection count in production

### Message Size Limits
- Default maximum message size: 32KB
- Large payloads should be split or transmitted via REST API
- Consider compression for repetitive data

### Recommended Patterns

#### Throttling Updates
```javascript
// Throttle rapid leaderboard updates
let leaderboardUpdateTimeout;
connection.on('LeaderboardUpdate', (data) => {
  clearTimeout(leaderboardUpdateTimeout);
  leaderboardUpdateTimeout = setTimeout(() => {
    updateLeaderboardDisplay(data.leaderboard);
  }, 100); // Max 10 updates per second
});
```

#### State Synchronization
```javascript
// Keep local state in sync with server events
let localSessionState = {
  participants: [],
  currentQuestion: null,
  leaderboard: []
};

connection.on('ParticipantJoined', (data) => {
  localSessionState.participants.push(data.participant);
  updateUI();
});
```

## Security Considerations

### Input Validation
- All client inputs are validated server-side
- Display names are sanitized to prevent XSS
- Answer submissions are validated against question options

### Rate Limiting
- Connection attempts: 10 per minute per IP
- Method calls: 100 per minute per connection
- Answer submissions: 1 per question per participant

### Authentication (Production)
```javascript
// JWT authentication (when implemented)
const connection = new HubConnectionBuilder()
  .withUrl('/hubs/hostedquiz', {
    accessTokenFactory: () => getJwtToken()
  })
  .build();
```

## Monitoring and Logging

### Server-Side Metrics
- Active connection count
- Messages per second
- Session creation/completion rates
- Error rates by method

### Client-Side Telemetry
```javascript
// Track connection health
let connectionMetrics = {
  connectTime: null,
  messagesSent: 0,
  messagesReceived: 0,
  errors: 0
};

connection.onreconnected(() => {
  connectionMetrics.connectTime = Date.now();
});
```

## Testing

### Unit Testing Hub Methods
```csharp
// Test example using xUnit and Moq
[Fact]
public async Task JoinSession_ValidJoinCode_ReturnsSuccess()
{
  var mockSessionManager = new Mock<ILiveSessionManager>();
  var hub = new HostedQuizHub(mockSessionManager.Object, logger);
  
  // Setup mock expectations
  // Execute method
  // Assert results
}
```

### Integration Testing
```javascript
// Use SignalR test server for integration tests
const connection = new HubConnectionBuilder()
  .withUrl(testServerUrl + '/hubs/hostedquiz')
  .build();

await connection.start();

// Test complete workflows
const createResult = await connection.invoke('CreateSession', 'test-quiz', 'Test Session');
// Assert results and test subsequent calls
```

This specification provides complete protocol documentation for implementing SignalR clients that interact with the VibeGuess live quiz system.