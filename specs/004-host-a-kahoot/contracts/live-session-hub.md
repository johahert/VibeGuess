# SignalR Hub Contract: HostedQuizHub

## Hub URL
`/hubs/hosted-quiz`

Connections require an access token (JWT) obtained from the REST endpoints. Host and player tokens encode their role and session ID.

## Groups
- **Session:{sessionId}** – all participants (host + players)
- **Session:{sessionId}:Host** – host connection only
- **Session:{sessionId}:Players** – all active player connections

## Client-to-Server Methods

| Method | Caller | Payload | Description | Validation |
|--------|--------|---------|-------------|------------|
| `RegisterHost` | Host | `{ sessionId: Guid }` | Binds host connection to the session, joins host group. | Token must match host user and session state must be Lobby/Paused. |
| `StartSession` | Host | `{ sessionId: Guid }` | Transitions lobby to first question, broadcasts `QuestionStarted`. | Session must be in Lobby and have ≥1 participant. |
| `AdvanceQuestion` | Host | `{ sessionId: Guid }` | Ends current question, triggers scoring, and starts the next one. | Session must be in RevealingAnswer or QuestionLive with elapsed deadline. |
| `RevealAnswer` | Host | `{ sessionId: Guid }` | Broadcasts correct answer and leaderboard update without advancing index. | Session must be in QuestionLive. |
| `EndSession` | Host | `{ sessionId: Guid }` | Forces session to Completed state and emits summary payload. | Allowed from any active state except Completed/Terminated. |
| `SubmitAnswer` | Player | `{ sessionId: Guid, questionId: Guid, answerOptionId: Guid? }` | Records player answer and emits acknowledgement to caller. | Must be before question deadline, participant must be Active. |
| `SendHeartbeat` | Player | `{ sessionId: Guid }` | Updates participant presence timestamp every 10 seconds. | Session must not be Completed/Terminated. |

## Server-to-Client Events

| Event | Recipients | Payload | Notes |
|-------|------------|---------|-------|
| `LobbyUpdated` | Host & Players | `{ sessionId, participants: ParticipantSummary[] }` | Fired when players join/leave or names change. |
| `QuestionStarted` | Players | `{ sessionId, questionId, orderIndex, prompt, answerOptions[], deadlineUtc, durationSeconds }` | Signals players to display the new question. |
| `AnswerRecorded` | Player (caller) | `{ questionId, acknowledgedAtUtc }` | Confirms answer receipt; no correctness info to prevent hints. |
| `LeaderboardUpdated` | Host & Players | `{ standings: StandingEntry[], questionIndex }` | Broadcast after scoring each question. |
| `AnswerReveal` | Host & Players | `{ questionId, correctOptionId, perOptionCounts[] }` | Provides aggregate answer breakdowns. |
| `SessionPaused` | Host & Players | `{ sessionId, resumeDeadlineUtc }` | Triggered when host disconnects and grace timer starts. |
| `SessionTerminated` | Host & Players | `{ sessionId, reason }` | Sent when grace period expires or host ends session prematurely. |
| `SessionCompleted` | Host & Players | `{ sessionId, summary }` | Includes podium data and analytics snapshot. |
| `ParticipantRemoved` | Host & Player | `{ participantId, reason }` | Directed to host and impacted player. |
| `ParticipantReinstated` | Host & Players | `{ participantId }` | Broadcast when blacklist entry is cleared. |

## Error Handling
- Validation failures return hub exceptions with `errorCode` and `details` fields.
- Unauthorized calls immediately close the connection with a `40103` code.
- Rate-limit per connection: maximum 10 `SubmitAnswer` calls per question to deter spamming.

## Presence & Timeouts
- Players missing 3 consecutive heartbeats transition to `Disconnected` and receive a warning via `ParticipantRemoved` event (reason: "timeout").
- Host presence is monitored server-side; on disconnect, `SessionPaused` event is emitted and the 30-second timer begins.
