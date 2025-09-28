# Data Model: Kahoot-Style Ho## LiveParticipant (In-Memory Only)
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
  - Status transitions: Connected→Disconnected/Removed, Removed→Connected (if unbanned)ssions

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

## Participant
- **Purpose**: Captures each player’s state within a session.
- **Fields**:
  - `Id` (Guid)
  - `SessionId` (Guid, FK → LiveQuizSession)
  - `DisplayName` (string, 1–32 chars)
  - `BaseName` (string, retains pre-suffix input)
  - `JoinOrder` (int)
  - `IsMuted` (bool)
  - `JoinState` (enum: Lobby, Active, Removed, Completed)
  - `Score` (int)
  - `Connected` (bool)
  - `LastHeartbeatUtc` (DateTime)
- **Relationships**:
  - 1 → many with `PlayerAnswer`
- **Validation**:
  - `DisplayName` auto-suffixed to avoid duplicates; `BaseName` retains original input.
  - `JoinState` transitions: Lobby→Active, Active→Removed|Completed, Removed→Active when reinstated.

## PlayerAnswer
- **Purpose**: Records a participant’s response per question during a session.
- **Fields**:
  - `Id` (Guid)
  - `SessionId` (Guid, FK → LiveQuizSession)
  - `ParticipantId` (Guid, FK → Participant)
  - `QuestionId` (Guid, FK → Quiz.Question)
  - `AnswerOptionId` (Guid?, FK → Quiz.AnswerOption)
  - `SubmittedAtUtc` (DateTime)
  - `IsCorrect` (bool)
  - `TimeRemainingSeconds` (int)
  - `ScoreAwarded` (int)
- **Validation**:
  - Only one `PlayerAnswer` per participant/question pair.
  - `ScoreAwarded` = 0 for incorrect answers.

## SessionQuestionState
- **Purpose**: Tracks per-question pacing and host-controlled settings.
- **Fields**:
  - `Id` (Guid)
  - `SessionId` (Guid, FK → LiveQuizSession)
  - `QuestionId` (Guid)
  - `ConfiguredDurationSeconds` (int, 10–120)
  - `RevealDurationSeconds` (int, default 5)
  - `OrderIndex` (int)
  - `Status` (enum: Pending, Live, Revealed, Skipped)
- **Validation**:
  - `ConfiguredDurationSeconds` must be within host-defined bounds.
  - `OrderIndex` mirrors quiz question order.

## SessionAnalytics
- **Purpose**: Stores post-session summary data for later insight.
- **Fields**:
  - `Id` (Guid)
  - `SessionId` (Guid, FK → LiveQuizSession, unique)
  - `TotalParticipants` (int)
  - `AverageAccuracy` (decimal, 0–100)
  - `AverageResponseTimeSeconds` (decimal)
  - `TopParticipants` (JSON array of podium results: display name + score)
  - `CompletedAtUtc` (DateTime)
- **Validation**:
  - Created when session transitions to Completed.

## Relationships Diagram (Textual)
- LiveQuizSession ⇄ Participant (1:N)
- LiveQuizSession ⇄ SessionQuestionState (1:N)
- Participant ⇄ PlayerAnswer (1:N)
- LiveQuizSession ⇄ PlayerAnswer (1:N)
- LiveQuizSession ⇄ SessionAnalytics (1:1)

## State Transition Highlights
- **Host Disconnect**: LiveQuizSession enters `Paused`, records `HostDisconnectedAtUtc`, and starts grace timer. Reconnection clears timer; otherwise transition to `Terminated`.
- **Session Start**: Lobby → QuestionLive when host triggers start; `CurrentQuestionIndex` set to 0 and `QuestionDeadlineUtc` computed.
- **Question Advance**: `QuestionDeadlineUtc` recalculated, `SessionQuestionState` updated to Live, previous question flagged as Revealed, participants without answers assigned ScoreAwarded=0.
