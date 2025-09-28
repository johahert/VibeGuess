# Data Model: Kahoot-Style Hosted Music Quiz Sessions

## LiveQuizSession
- **Purpose**: Represents a real-time hosted instance of an existing quiz.
- **Fields**:
  - `Id` (Guid)
  - `QuizId` (Guid, FK → Quiz)
  - `HostUserId` (Guid)
  - `JoinCode` (string, 6–8 characters, unique per active session)
  - `State` (enum: Lobby, Paused, QuestionLive, RevealingAnswer, Completed, Terminated)
  - `CurrentQuestionIndex` (int, null when lobby)
  - `QuestionDeadlineUtc` (DateTime?, populated during QuestionLive state)
  - `CreatedAtUtc` (DateTime)
  - `UpdatedAtUtc` (DateTime)
  - `HostDisconnectedAtUtc` (DateTime?, set when host connection drops)
  - `Blacklist` (collection of Participant.Id values)
- **Relationships**:
  - 1 → many with `Participant`
  - 1 → many with `PlayerAnswer`
  - 1 → 1 with `SessionAnalytics`
- **Validation**:
  - `JoinCode` must be unique among active sessions.
  - `State` transitions follow allowed graph (Lobby→QuestionLive→RevealingAnswer→QuestionLive|Completed, Pause accessible from QuestionLive, Terminated reachable from any state).

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
