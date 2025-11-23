# VibeGuess Live Quiz Sessions API Documentation

## Overview

The VibeGuess live quiz system provides real-time multiplayer quiz functionality using a combination of REST endpoints and SignalR WebSockets. The primary interaction happens via SignalR for real-time gameplay, while REST endpoints provide supplementary functionality for session management and analytics.

## Base Configuration

- **API Base URL**: `https://localhost:7009/api` (Development)
- **SignalR Hub URL**: `https://localhost:7009/hubs/hostedquiz`
- **Redis Required**: Yes (for session state management)

---

## REST API Endpoints

### 1. Create Hosted Session

Creates a new live quiz session that participants can join.

**Endpoint**: `POST /api/hosted-sessions`

**Request Headers**:
```
Content-Type: application/json
```

**Request Body**:
```typescript
interface CreateHostedSessionRequest {
  quizId: string;           // Required: Quiz ID to host (max 100 chars)
  title: string;            // Required: Session title (max 200 chars)  
  questionTimeLimit: number; // Optional: Time limit per question in seconds (10-300, default: 30)
}
```

**Example Request**:
```json
{
  "quizId": "quiz-123",
  "title": "Friday Music Quiz",
  "questionTimeLimit": 45
}
```

**Success Response** (200 OK):
```typescript
interface CreateHostedSessionResponse {
  sessionId: string;    // Unique session identifier
  joinCode: string;     // 6-character code for participants to join
  title: string;        // Session title
  state: string;        // Session state: "Lobby" | "Active" | "Paused" | "Completed"
  createdAt: string;    // ISO 8601 timestamp
}
```

**Example Success Response**:
```json
{
  "sessionId": "session-uuid-123",
  "joinCode": "ABC123",
  "title": "Friday Music Quiz",
  "state": "Lobby",
  "createdAt": "2025-09-28T10:30:00Z"
}
```

**Error Responses**:
- **400 Bad Request**: Invalid request parameters
- **500 Internal Server Error**: Failed to create session

---

### 2. Get Session Information

Retrieves current information about a live session by join code.

**Endpoint**: `GET /api/hosted-sessions/{joinCode}`

**Path Parameters**:
- `joinCode` (string): 6-character session join code

**Example Request**: `GET /api/hosted-sessions/ABC123`

**Success Response** (200 OK):
```typescript
interface HostedSessionInfoResponse {
  sessionId: string;
  joinCode: string;
  title: string;
  state: "Lobby" | "Active" | "Paused" | "Completed";
  currentQuestionIndex: number;
  participantCount: number;
  createdAt: string;
  startedAt: string | null;
  endedAt: string | null;
  questionTimeLimit: number;
  participants: ParticipantSummary[];
}

interface ParticipantSummary {
  participantId: string;
  displayName: string;
  score: number;
  correctAnswers: number;
  totalAnswers: number;
  isConnected: boolean;
  joinedAt: string;
  accuracy: number;  // Calculated accuracy percentage
}
```

**Example Success Response**:
```json
{
  "sessionId": "session-uuid-123",
  "joinCode": "ABC123",
  "title": "Friday Music Quiz",
  "state": "Active",
  "currentQuestionIndex": 2,
  "participantCount": 5,
  "createdAt": "2025-09-28T10:30:00Z",
  "startedAt": "2025-09-28T10:35:00Z",
  "endedAt": null,
  "questionTimeLimit": 45,
  "participants": [
    {
      "participantId": "participant-123",
      "displayName": "Alice",
      "score": 250,
      "correctAnswers": 2,
      "totalAnswers": 2,
      "isConnected": true,
      "joinedAt": "2025-09-28T10:32:00Z",
      "accuracy": 100.0
    }
  ]
}
```

**Error Responses**:
- **400 Bad Request**: Join code is required
- **404 Not Found**: Session not found
- **500 Internal Server Error**: Failed to retrieve session

---

### 3. Get Session Summary (Analytics)

Retrieves comprehensive analytics for a session, typically used after completion.

**Endpoint**: `GET /api/hosted-sessions/{sessionId}/summary`

**Path Parameters**:
- `sessionId` (string): Unique session identifier

**Example Request**: `GET /api/hosted-sessions/session-uuid-123/summary`

**Success Response** (200 OK):
```typescript
interface SessionSummaryResponse {
  sessionId: string;
  title: string;
  createdAt: string;
  startedAt: string | null;
  endedAt: string | null;
  duration: string | null;  // Duration in ISO 8601 format (e.g., "PT15M30S")
  stats: SessionStats;
  finalLeaderboard: ParticipantSummary[];  // Top 20 participants
  questionStats: QuestionStats[];
}

interface SessionStats {
  totalParticipants: number;
  totalQuestions: number;
  totalAnswers: number;
  averageScore: number;
  averageAccuracy: number;
  averageResponseTime: string;  // Duration in ISO 8601 format
}

interface QuestionStats {
  questionIndex: number;
  totalAnswers: number;
  correctAnswers: number;
  accuracyPercentage: number;
  averageResponseTime: string;  // Duration in ISO 8601 format
}
```

**Example Success Response**:
```json
{
  "sessionId": "session-uuid-123",
  "title": "Friday Music Quiz",
  "createdAt": "2025-09-28T10:30:00Z",
  "startedAt": "2025-09-28T10:35:00Z",
  "endedAt": "2025-09-28T10:50:00Z",
  "duration": "PT15M",
  "stats": {
    "totalParticipants": 5,
    "totalQuestions": 10,
    "totalAnswers": 48,
    "averageScore": 180.5,
    "averageAccuracy": 72.5,
    "averageResponseTime": "PT8S"
  },
  "finalLeaderboard": [
    {
      "participantId": "participant-123",
      "displayName": "Alice",
      "score": 450,
      "correctAnswers": 8,
      "totalAnswers": 10,
      "isConnected": true,
      "joinedAt": "2025-09-28T10:32:00Z",
      "accuracy": 80.0
    }
  ],
  "questionStats": [
    {
      "questionIndex": 0,
      "totalAnswers": 5,
      "correctAnswers": 3,
      "accuracyPercentage": 60.0,
      "averageResponseTime": "PT12S"
    }
  ]
}
```

**Error Responses**:
- **400 Bad Request**: Session ID is required
- **404 Not Found**: Session not found
- **500 Internal Server Error**: Failed to retrieve session summary

---

## SignalR WebSocket Methods

### Connection Setup

```typescript
// JavaScript/TypeScript connection setup
import { HubConnectionBuilder } from '@microsoft/signalr';

const connection = new HubConnectionBuilder()
  .withUrl('/hubs/hostedquiz')
  .build();

await connection.start();
```

### Host Methods

These methods are intended for the quiz host (session creator).

#### 1. CreateSession

Creates a new live quiz session via SignalR.

**Method**: `CreateSession`

**Parameters**:
```typescript
await connection.invoke('CreateSession', quizId: string, title: string)
```

**Returns**:
```typescript
interface CreateSessionResponse {
  success: boolean;
  sessionId?: string;    // Present if success = true
  joinCode?: string;     // Present if success = true
  error?: string;        // Present if success = false
}
```

**Example Usage**:
```javascript
const result = await connection.invoke('CreateSession', 'quiz-123', 'Friday Music Quiz');
if (result.success) {
  console.log(`Session created: ${result.sessionId}, Join code: ${result.joinCode}`);
} else {
  console.error(`Failed to create session: ${result.error}`);
}
```

---

#### 2. StartGame

Starts the quiz game, moving from lobby to active state.

**Method**: `StartGame`

**Parameters**:
```typescript
await connection.invoke('StartGame', sessionId: string)
```

**Returns**:
```typescript
interface StartGameResponse {
  success: boolean;
  error?: string;  // Present if success = false
}
```

**Broadcasts**: Sends `GameStarted` event to all participants.

**Example Usage**:
```javascript
const result = await connection.invoke('StartGame', 'session-uuid-123');
if (result.success) {
  console.log('Game started successfully');
} else {
  console.error(`Failed to start game: ${result.error}`);
}
```

---

#### 3. NextQuestion

Advances to the next question in the quiz.

**Method**: `NextQuestion`

**Parameters**:
```typescript
await connection.invoke('NextQuestion', sessionId: string, questionIndex: number, questionData: object)
```

**Question Data Structure**:
```typescript
interface QuestionData {
  questionId: string;
  questionText: string;
  options: string[];      // Array of answer options
  correctAnswer: string;  // The correct answer (sent to host only)
  timeLimit?: number;     // Override default time limit for this question
  type: "multiple-choice" | "true-false" | "text";
}
```

**Returns**:
```typescript
interface NextQuestionResponse {
  success: boolean;
  error?: string;  // Present if success = false
}
```

**Broadcasts**: Sends `NewQuestion` event to all participants with question data (without correct answer).

**Example Usage**:
```javascript
const questionData = {
  questionId: "q1",
  questionText: "Which artist sang 'Bohemian Rhapsody'?",
  options: ["Queen", "The Beatles", "Led Zeppelin", "Pink Floyd"],
  correctAnswer: "Queen",
  type: "multiple-choice"
};

const result = await connection.invoke('NextQuestion', 'session-uuid-123', 1, questionData);
```

---

#### 4. EndSession

Ends the quiz session and shows final results.

**Method**: `EndSession`

**Parameters**:
```typescript
await connection.invoke('EndSession', sessionId: string)
```

**Returns**:
```typescript
interface EndSessionResponse {
  success: boolean;
  error?: string;  // Present if success = false
}
```

**Broadcasts**: Sends `GameEnded` event to all participants with final leaderboard.

**Example Usage**:
```javascript
const result = await connection.invoke('EndSession', 'session-uuid-123');
```

---

#### 5. RemovePlayer

Removes a participant from the session (host moderation).

**Method**: `RemovePlayer`

**Parameters**:
```typescript
await connection.invoke('RemovePlayer', sessionId: string, participantId: string)
```

**Returns**:
```typescript
interface RemovePlayerResponse {
  success: boolean;
  error?: string;  // Present if success = false
}
```

**Broadcasts**: Sends `RemovedFromSession` to the removed participant and `ParticipantLeft` to others.

---

### Player Methods

These methods are for quiz participants.

#### 1. JoinSession

Joins a live quiz session using a join code.

**Method**: `JoinSession`

**Parameters**:
```typescript
await connection.invoke('JoinSession', joinCode: string, displayName: string)
```

**Returns**:
```typescript
interface JoinSessionResponse {
  success: boolean;
  sessionId?: string;           // Present if success = true
  participantId?: string;       // Present if success = true
  sessionState?: "Lobby" | "Active" | "Paused" | "Completed";
  currentQuestionIndex?: number; // Present if success = true
  error?: string;               // Present if success = false
}
```

**Broadcasts**: Sends `ParticipantJoined` event to host and other participants.

**Example Usage**:
```javascript
const result = await connection.invoke('JoinSession', 'ABC123', 'Alice');
if (result.success) {
  console.log(`Joined session: ${result.sessionId} as participant: ${result.participantId}`);
} else {
  console.error(`Failed to join: ${result.error}`);
}
```

---

#### 2. SubmitAnswer

Submits an answer to the current question.

**Method**: `SubmitAnswer`

**Parameters**:
```typescript
await connection.invoke('SubmitAnswer', sessionId: string, participantId: string, selectedAnswer: string)
```

**Returns**:
```typescript
interface SubmitAnswerResponse {
  success: boolean;
  isCorrect?: boolean;    // Present if success = true
  score?: number;         // Points earned for this answer
  totalScore?: number;    // Total accumulated score
  error?: string;         // Present if success = false
}
```

**Broadcasts**: Sends `AnswerSubmitted` to host and `LeaderboardUpdate` to all participants.

**Example Usage**:
```javascript
const result = await connection.invoke('SubmitAnswer', 'session-uuid-123', 'participant-123', 'Queen');
if (result.success) {
  console.log(`Answer ${result.isCorrect ? 'correct' : 'incorrect'}! Score: ${result.score}`);
}
```

---

#### 3. LeaveSession

Leaves the current session.

**Method**: `LeaveSession`

**Parameters**:
```typescript
await connection.invoke('LeaveSession', sessionId: string, participantId: string)
```

**Returns**:
```typescript
interface LeaveSessionResponse {
  success: boolean;
  error?: string;  // Present if success = false
}
```

**Broadcasts**: Sends `ParticipantLeft` event to remaining participants.

---

### Server-to-Client Events

These events are broadcast from the server to connected clients.

#### 1. GameStarted

Sent when the host starts the game.

**Event**: `GameStarted`

**Payload**:
```typescript
interface GameStartedEvent {
  sessionId: string;
}
```

**Recipients**: All participants in the session

**Example Handler**:
```javascript
connection.on('GameStarted', (data) => {
  console.log(`Game started for session: ${data.sessionId}`);
  // Transition UI to game mode
});
```

---

#### 2. NewQuestion

Sent when the host advances to a new question.

**Event**: `NewQuestion`

**Payload**:
```typescript
interface NewQuestionEvent {
  sessionId: string;
  questionIndex: number;
  question: {
    questionId: string;
    questionText: string;
    options: string[];
    type: "multiple-choice" | "true-false" | "text";
    // Note: correctAnswer is NOT included for participants
  };
  timeLimit: number;  // Time limit in seconds
}
```

**Recipients**: All participants in the session

**Example Handler**:
```javascript
connection.on('NewQuestion', (data) => {
  console.log(`New question ${data.questionIndex}: ${data.question.questionText}`);
  // Display question and start timer
  startQuestionTimer(data.timeLimit);
});
```

---

#### 3. GameEnded

Sent when the host ends the session.

**Event**: `GameEnded`

**Payload**:
```typescript
interface GameEndedEvent {
  sessionId: string;
  leaderboard: Array<{
    displayName: string;
    score: number;
    correctAnswers: number;
    totalAnswers: number;
    accuracy: number;
  }>;  // Top 10 participants
}
```

**Recipients**: All participants in the session

**Example Handler**:
```javascript
connection.on('GameEnded', (data) => {
  console.log('Game ended! Final leaderboard:', data.leaderboard);
  // Show final results screen
});
```

---

#### 4. ParticipantJoined

Sent when a new participant joins the session.

**Event**: `ParticipantJoined`

**Payload**:
```typescript
interface ParticipantJoinedEvent {
  sessionId: string;
  participant: {
    participantId: string;
    displayName: string;
  };
  participantCount: number;
}
```

**Recipients**: Host and existing participants (excluding the newly joined participant)

---

#### 5. ParticipantLeft

Sent when a participant leaves or is removed from the session.

**Event**: `ParticipantLeft`

**Payload**:
```typescript
interface ParticipantLeftEvent {
  sessionId: string;
  participantId: string;
  participantCount: number;
}
```

**Recipients**: All remaining participants and host

---

#### 6. AnswerSubmitted

Sent to the host when a participant submits an answer.

**Event**: `AnswerSubmitted`

**Payload**:
```typescript
interface AnswerSubmittedEvent {
  sessionId: string;
  participantId: string;
  participantName: string;
  questionIndex: number;
  hasAnswered: boolean;  // Always true for this event
}
```

**Recipients**: Host only

---

#### 7. LeaderboardUpdate

Sent when scores are updated after answer submissions.

**Event**: `LeaderboardUpdate`

**Payload**:
```typescript
interface LeaderboardUpdateEvent {
  sessionId: string;
  leaderboard: Array<{
    participantId: string;
    displayName: string;
    score: number;
    correctAnswers: number;
    totalAnswers: number;
  }>;  // Top 10 participants
}
```

**Recipients**: All participants in the session

---

#### 8. RemovedFromSession

Sent to a participant when they are removed by the host.

**Event**: `RemovedFromSession`

**Payload**:
```typescript
interface RemovedFromSessionEvent {
  sessionId: string;
  reason: string;  // e.g., "Removed by host"
}
```

**Recipients**: The removed participant only

---

## Session State Flow

```
1. Host creates session (Lobby state)
2. Participants join using join code
3. Host starts game (Active state)
4. Host sends questions one by one
5. Participants submit answers
6. Leaderboard updates in real-time
7. Host ends session (Completed state)
8. Final results are shown
```

## Error Handling

### Common Error Scenarios

1. **Session Not Found**: Join code doesn't exist or session has expired
2. **Unauthorized**: Trying to perform host actions without being the host
3. **Invalid State**: Trying to perform actions in wrong session state
4. **Duplicate Display Name**: Display name already taken in session
5. **Already Answered**: Participant trying to answer same question twice
6. **Connection Issues**: Network disconnections during gameplay

### Recommended Frontend Error Handling

```javascript
// Handle connection errors
connection.onclose((error) => {
  console.error('Connection lost:', error);
  // Show reconnection UI
});

// Handle method call errors
try {
  const result = await connection.invoke('JoinSession', joinCode, displayName);
  if (!result.success) {
    // Show user-friendly error message
    showError(result.error);
  }
} catch (error) {
  // Handle network/connection errors
  showError('Connection error. Please try again.');
}
```

## Rate Limiting

- All REST endpoints include rate limiting headers
- Default limit: 5 requests per minute per IP
- Headers returned: `X-RateLimit-Remaining`, `X-RateLimit-Reset`

## CORS Configuration

The API is configured to allow requests from:
- `http://localhost:3000` (Create React App)
- `http://localhost:5173` (Vite)
- `https://localhost:3000`
- `https://localhost:5173`

Credentials are enabled for SignalR WebSocket connections.

## Redis Requirements

- Redis server must be running on `localhost:6379` (configurable)
- Session data is automatically cleaned up via TTL:
  - Lobby sessions: 2 hours
  - Active sessions: 1 hour  
  - Completed sessions: 30 minutes

## TypeScript Types

For TypeScript projects, you can create these interfaces:

```typescript
// Add all the interfaces from above sections
// These provide full type safety for your frontend implementation
```

This documentation provides everything needed for frontend developers to integrate with the VibeGuess live quiz system using both REST API calls and SignalR WebSocket connections.