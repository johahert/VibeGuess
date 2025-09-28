# Data Model: Kahoot-Style Hosted Music Quiz Sessions

**Architecture**: In-Memory State Management with Optional Result Persistence

## LiveQuizSession (In-Memory Only)
- **Storage**: Redis cache with expiration, NOT persisted to database
- **Purpose**: Represents a real-time hosted instance of an existing quiz
- **Cache Key**: `live-session:{JoinCode}` and `live-session:host:{HostUserId}`
- **Fields**:
  - `Id` (Guid)
  - `QuizId` (Guid, references existing Quiz entity)
  - `HostUserId` (Guid)
  - `HostConnectionId` (string, SignalR connection tracking)
  - `JoinCode` (string, 6 characters, unique among active sessions)
  - `State` (enum: Lobby, InProgress, Paused, Completed, Terminated)
  - `CurrentQuestionIndex` (int, -1 when in lobby)
  - `QuestionStartedAt` (DateTime?, when current question began)
  - `QuestionTimeLimit` (int, seconds per question, host configurable)
  - `CreatedAt` (DateTime)
  - `LastActivity` (DateTime)
  - `HostDisconnectedAt` (DateTime?, grace period tracking)
  - `Participants` (Dictionary<Guid, LiveParticipant>)
  - `Blacklist` (HashSet<string>, blocked display names)
  - `CurrentAnswers` (Dictionary<Guid, LiveAnswer>, current question responses)
- **Expiration**: Auto-cleanup after 4 hours of inactivity
- **Validation**:
  - `JoinCode` collision detection across active sessions
  - State transitions: Lobby→InProgress→Completed, Paused accessible from InProgress

## LiveParticipant (In-Memory Only)
- **Storage**: Nested within LiveQuizSession, NOT persisted to database
- **Purpose**: Captures each player's real-time state within a session
- **Fields**:
  - `Id` (Guid)
  - `DisplayName` (string, 1-32 chars, auto-suffixed for uniqueness)
  - `ConnectionId` (string, SignalR connection tracking)
  - `Status` (enum: Connected, Disconnected, Removed)
  - `Score` (int, real-time calculated)
  - `CorrectAnswers` (int)
  - `JoinedAt` (DateTime)
  - `LastSeen` (DateTime)
  - `IsBlacklisted` (bool)
- **Validation**:
  - `DisplayName` auto-suffixed to avoid duplicates (Player, Player2, Player3)
  - Status transitions: Connected→Disconnected/Removed, Removed→Connected (if unbanned)

## LiveAnswer (In-Memory Only)
- **Storage**: Temporarily stored in LiveQuizSession.CurrentAnswers, NOT persisted
- **Purpose**: Records participant's response to current question in real-time
- **Fields**:
  - `ParticipantId` (Guid)
  - `QuestionId` (Guid)
  - `AnswerOptionId` (Guid?, selected multiple choice option)
  - `SubmittedAt` (DateTime)
  - `ResponseTimeMs` (double, time to answer)
  - `IsCorrect` (bool)
  - `PointsAwarded` (int, 100 + time bonus)
- **Lifecycle**: Created during question, scored in real-time, discarded after question ends
- **Validation**:
  - One answer per participant per question
  - Points calculated: 100 base + (seconds remaining) for correct answers

## SessionSummary (Optional Database Persistence)
- **Storage**: Database table for historical analytics (optional)
- **Purpose**: Stores final session results for analytics and leaderboards
- **Fields**:
  - `Id` (Guid)
  - `QuizId` (Guid, references Quiz)
  - `HostUserId` (Guid)
  - `ParticipantCount` (int)
  - `CompletedAt` (DateTime)
  - `DurationMinutes` (int)
  - `AverageScore` (decimal)
  - `TopScores` (JSON, top 10 participants with scores)
- **Created**: When session completes successfully
- **Usage**: Historical analytics, host dashboard, leaderboards

## Architecture Summary

### In-Memory Components (Redis Cache)
- **LiveQuizSession**: Primary game state
- **LiveParticipant**: Player management  
- **LiveAnswer**: Current question responses
- **Cache Keys**: 
  - `session:{joinCode}` → LiveQuizSession
  - `host-session:{hostUserId}` → session lookup
- **Expiration**: 4 hours auto-cleanup

### Optional Database Components
- **SessionSummary**: Final results only
- **Existing Quiz/Question/AnswerOption**: Referenced, not modified

### State Management Flow
1. **Session Creation**: Store in Redis with generated join code
2. **Player Join**: Add to session.Participants dictionary
3. **Gameplay**: Update scores in real-time, store current answers temporarily
4. **Question End**: Calculate final scores, clear current answers
5. **Session End**: Optionally persist summary, clear all Redis data

### SignalR Message Flow
- **Host Actions**: CreateSession, StartGame, NextQuestion, EndSession, RemovePlayer
- **Player Actions**: JoinSession, SubmitAnswer, LeaveSession
- **Broadcast Events**: PlayerJoined, QuestionStarted, AnswerReceived, ScoreUpdate, GameEnded

## JSON Schema Examples

### LiveQuizSession (Redis Storage)
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "quizId": "quiz-123",
  "hostUserId": "host-456",
  "hostConnectionId": "conn-789",
  "joinCode": "ABC123",
  "state": "InProgress",
  "currentQuestionIndex": 2,
  "questionStartedAt": "2025-09-28T10:30:00Z",
  "questionTimeLimit": 30,
  "participants": {
    "player-1": {
      "id": "player-1",
      "displayName": "Alice",
      "connectionId": "conn-abc",
      "status": "Connected",
      "score": 250,
      "correctAnswers": 2
    }
  },
  "currentAnswers": {
    "player-1": {
      "participantId": "player-1",
      "questionId": "q3",
      "answerOptionId": "opt-b",
      "submittedAt": "2025-09-28T10:30:15Z",
      "isCorrect": true,
      "pointsAwarded": 115
    }
  }
}
```

### SessionSummary (Database Persistence)
```json
{
  "id": "summary-123",
  "quizId": "quiz-123",
  "hostUserId": "host-456",
  "participantCount": 25,
  "completedAt": "2025-09-28T11:00:00Z",
  "durationMinutes": 15,
  "averageScore": 340.5,
  "topScores": [
    {"displayName": "Alice", "score": 890},
    {"displayName": "Bob", "score": 850},
    {"displayName": "Charlie", "score": 820}
  ]
}
```