# Tasks: Kahoot-Style Hosted Music Quiz Sessions# Task Plan: Kahoot-Style Hosted Music Quiz Sessions



**Input**: Design documents from `/specs/004-host-a-kahoot/`## Parallel Execution Examples

**Prerequisites**: plan.md, research.md, data-model.md, contracts/, quickstart.md- `task run T003 T004` — generate REST + hub contract tests together before implementation

- `task run T007 T008` — create Participant and PlayerAnswer models in parallel

## Execution Flow (main)- `task run T015 T016` — build host moderation endpoints alongside reinstate flow once service layer exists

```- `task run T020 T021` — integration tests for disconnect and analytics can execute concurrently after core gameplay is in place

1. Load plan.md → Extract SignalR + Redis in-memory architecture

2. Load data-model.md → Extract LiveQuizSession, LiveParticipant, LiveAnswer models  ## Tasks

3. Load contracts/ → Extract REST endpoints + SignalR hub methods

4. Load research.md → Extract Redis backplane, scoring decisions### Setup & Infrastructure

5. Generate tasks: Setup → Tests (TDD) → Models → Services → Hub → Endpoints → Polish- **T001** — Audit solution dependencies for SignalR/Redis support; update `src/VibeGuess.Api/VibeGuess.Api.csproj` reference list and appsettings scaffolding if needed. *(Sequential foundation)*

6. Apply [P] markers for parallel execution (different files)- **T002** — Configure development Redis backplane settings (`appsettings.Development.json`) and document connection string expectations. *(Depends on T001)*

7. Ensure all live session data stays in Redis (NO database persistence)

```### Contract Tests (TDD First) [P]

- **T003 [P]** — Author failing contract tests in `tests/VibeGuess.Api.Tests/HostedSessions/CreateHostedSessionTests.cs` covering `POST /api/hosted-sessions` schema, auth, and conflict cases. *(Depends on T002)*

## Format: `[ID] [P?] Description`- **T004 [P]** — Author failing contract tests in `tests/VibeGuess.Api.Tests/HostedSessions/JoinHostedSessionTests.cs` for join flow, duplicate names, and blacklist rejection. *(Depends on T002)*

- **[P]**: Can run in parallel (different files, no dependencies)- **T005 [P]** — Author failing contract tests in `tests/VibeGuess.Api.Tests/HostedSessions/ModerationTests.cs` for remove/reinstate endpoints. *(Depends on T002)*

- Include exact file paths in descriptions- **T006 [P]** — Author failing contract tests in `tests/VibeGuess.Api.Tests/HostedSessions/SummaryTests.cs` verifying analytics payload and 404 handling. *(Depends on T002)*

- **T007 [P]** — Author failing hub contract tests in `tests/VibeGuess.Api.Tests/HostedSessions/HostedQuizHubContractTests.cs` validating SignalR methods/events. *(Depends on T002)*

## Phase 3.1: Setup & Dependencies

### Domain Models & Persistence [P]

- [ ] **T001** Add Redis dependencies to `src/VibeGuess.Api/VibeGuess.Api.csproj`- **T008 [P]** — Implement `LiveQuizSession` entity and EF configuration (`src/VibeGuess.Core/Entities/LiveQuizSession.cs`, `src/VibeGuess.Infrastructure/Data/Configurations/LiveQuizSessionConfiguration.cs`). *(Depends on T003–T007)*

  - Add Microsoft.AspNetCore.SignalR.StackExchangeRedis- **T009 [P]** — Implement `Participant` entity and configuration with duplicate-name suffix logic. *(Depends on T003–T007)*

  - Add Microsoft.Extensions.Caching.StackExchangeRedis- **T010 [P]** — Implement `PlayerAnswer` entity and configuration ensuring uniqueness per participant/question. *(Depends on T003–T007)*

  - Add StackExchange.Redis packages- **T011 [P]** — Implement `SessionQuestionState` entity for pacing metadata. *(Depends on T003–T007)*

- **T012 [P]** — Implement `SessionAnalytics` entity and configuration for post-session summary persistence. *(Depends on T003–T007)*

- [ ] **T002** Configure Redis connection in `src/VibeGuess.Api/appsettings.json` and `src/VibeGuess.Api/appsettings.Development.json`- **T013** — Create EF Core migration adding hosted session tables and relationships, ensuring blacklist stored via join-table or JSON column per design. *(Depends on T008–T012)*

  - Add Redis connection string configuration section

  - Set appropriate timeouts and connection options### Application Services & Infrastructure

- **T014** — Extend `IHostedSessionService` (new service) with session lifecycle operations: create lobby, join player, moderation, analytics retrieval. Implement in `src/VibeGuess.Api/Services/HostedSessions`. *(Depends on T008–T013)*

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3- **T015** — Implement `HostedSessionsController` REST endpoints (`POST /api/hosted-sessions`, `/join`, moderation, summary`) using new service and ensuring T003–T006 tests pass. *(Depends on T014)*

**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**- **T016** — Register Redis backplane in `Program.cs` and configure SignalR hub routing (`/hubs/hosted-quiz`). *(Depends on T014)*



- [ ] **T003** [P] REST contract test for POST /api/hosted-sessions in `tests/VibeGuess.Api.Tests/Controllers/HostedSessionsControllerTests.cs`### SignalR Hub & Real-Time Flow

  - Test session creation with valid quiz ID- **T017** — Implement `HostedQuizHub` class with methods/events from contract, including host registration, pacing commands, answer submission, and blacklist checks. *(Depends on T014, T016)*

  - Test authentication requirement- **T018** — Wire hub to session service for scoring (100 + bonus per unused second) and state transitions; ensure redispatched leaderboard/broadcast events align with contract. *(Depends on T017)*

  - Test quiz not found scenario

  - Test host already has active session conflict### Integration & Scenario Tests [P]

- **T019 [P]** — Build SignalR integration tests simulating host + players for lobby creation and question flow using TestServer, ensuring contract events are emitted. *(Depends on T017–T018)*

- [ ] **T004** [P] REST contract test for GET /api/hosted-sessions/{joinCode} in `tests/VibeGuess.Api.Tests/Controllers/HostedSessionsControllerTests.cs`  - **T020 [P]** — Integration test covering host disconnect pause/termination timing and reconnection grace period. *(Depends on T017–T018)*

  - Test valid join code returns session info- **T021 [P]** — Integration test verifying analytics summary persisted and returned by `GET /api/hosted-sessions/{sessionId}/summary`. *(Depends on T015, T018)*

  - Test invalid join code returns 404

  - Test session in progress returns 410 (no late joins)### Observability & Cleanup

- **T022** — Add structured logging and metrics for session lifecycle (creation, start, advance, timeout) and blacklist actions. *(Depends on T015–T018)*

- [ ] **T005** [P] REST contract test for GET /api/hosted-sessions/{sessionId}/summary in `tests/VibeGuess.Api.Tests/Controllers/HostedSessionsControllerTests.cs`- **T023** — Update documentation: `quickstart.md` validation steps, API reference, and README session hosting section. *(Depends on T015–T021)*

  - Test host can retrieve session summary after completion- **T024** — Final verification: run full test suite (`dotnet test`), review migration script, and prepare changelog entry for release notes. *(Depends on T008–T023)*

  - Test non-host gets 403 forbidden
  - Test session not found returns 404

- [ ] **T006** [P] SignalR Hub integration test for host methods in `tests/VibeGuess.Api.Tests/Hubs/HostedQuizHubTests.cs`
  - Test RegisterHost with valid token
  - Test StartSession transitions lobby to first question
  - Test AdvanceQuestion progresses game state
  - Test EndSession completes session and broadcasts summary

- [ ] **T007** [P] SignalR Hub integration test for player methods in `tests/VibeGuess.Api.Tests/Hubs/HostedQuizHubTests.cs`
  - Test player join session without authentication
  - Test SubmitAnswer records response and updates scores
  - Test SendHeartbeat updates participant presence
  - Test real-time event broadcasting (PlayerJoined, QuestionStarted)

- [ ] **T008** [P] Live session manager unit tests in `tests/VibeGuess.Api.Tests/Services/LiveSessionManagerTests.cs`
  - Test session creation with unique join codes
  - Test participant join with duplicate name handling
  - Test scoring calculation (100 base + time bonus)
  - Test session expiration and cleanup

## Phase 3.3: Core Models (In-Memory Only) - ONLY after tests are failing

- [ ] **T009** [P] LiveQuizSession model in `src/VibeGuess.Core/Models/LiveQuizSession.cs`
  - In-memory session state with all fields from data-model.md
  - State enum (Lobby, InProgress, Paused, Completed, Terminated)
  - Participants dictionary and blacklist collections
  - JSON serialization attributes for Redis storage

- [ ] **T010** [P] LiveParticipant model in `src/VibeGuess.Core/Models/LiveParticipant.cs`
  - Participant state with connection tracking
  - Status enum (Connected, Disconnected, Removed)
  - Real-time score and statistics fields
  - Display name collision handling logic

- [ ] **T011** [P] LiveAnswer model in `src/VibeGuess.Core/Models/LiveAnswer.cs`
  - Temporary answer storage for current question
  - Response time tracking and scoring fields
  - Validation for one answer per participant per question

- [ ] **T012** [P] SessionSummary entity (optional database persistence) in `src/VibeGuess.Core/Entities/SessionSummary.cs`
  - Final session results for historical analytics
  - JSON field for top participant scores
  - EF Core configuration and mapping

## Phase 3.4: Session Management Service

- [ ] **T013** ILiveSessionManager interface in `src/VibeGuess.Api/Services/ILiveSessionManager.cs`
  - Define Redis-based session state management contract
  - Session CRUD operations with join code generation
  - Participant management and real-time scoring methods
  - Connection tracking and cleanup operations

- [ ] **T014** LiveSessionManager implementation in `src/VibeGuess.Api/Services/LiveSessionManager.cs`
  - Redis cache integration for session storage
  - Join code generation with collision detection
  - Participant join with duplicate name auto-suffixing
  - Real-time scoring calculator (100 + time bonus)
  - Session cleanup and TTL management

## Phase 3.5: SignalR Hub Implementation

- [ ] **T015** HostedQuizHub class in `src/VibeGuess.Api/Hubs/HostedQuizHub.cs`
  - Inherit from Hub with authentication for host methods
  - Implement connection/disconnection handling with grace periods
  - Host methods: RegisterHost, StartSession, AdvanceQuestion, EndSession
  - Player methods: JoinAsPlayer, SubmitAnswer, SendHeartbeat
  - Real-time event broadcasting to groups and individuals

## Phase 3.6: REST API Endpoints

- [ ] **T016** HostedSessionsController in `src/VibeGuess.Api/Controllers/HostedSessionsController.cs`
  - POST /api/hosted-sessions (create session, return join code)
  - GET /api/hosted-sessions/{joinCode} (session info for players)
  - GET /api/hosted-sessions/{sessionId}/summary (post-session analytics)
  - Integration with LiveSessionManager service

## Phase 3.7: Integration & Configuration

- [ ] **T017** Configure SignalR with Redis backplane in `src/VibeGuess.Api/Program.cs`
  - Add Redis cache services configuration
  - Configure SignalR with StackExchange Redis backplane
  - Register ILiveSessionManager service in DI container
  - Map HostedQuizHub to /hubs/hosted-quiz route

- [ ] **T018** Add SessionSummary to EF Core DbContext in `src/VibeGuess.Infrastructure/Data/VibeGuessDbContext.cs`
  - Add DbSet for optional analytics persistence
  - Configure entity relationships and JSON serialization
  - Create database migration for SessionSummary table

## Phase 3.8: Polish & Validation

- [ ] **T019** [P] Connection management and error handling in `src/VibeGuess.Api/Hubs/HostedQuizHub.cs`
  - Host disconnect grace period (30 seconds)
  - Player timeout handling (3 missed heartbeats)
  - Hub exception handling with proper error codes
  - Rate limiting for SubmitAnswer method

- [ ] **T020** [P] Session cleanup background service in `src/VibeGuess.Api/Services/SessionCleanupService.cs`
  - Background service to clean expired sessions
  - Redis TTL monitoring and manual cleanup
  - Orphaned session detection and removal

- [ ] **T021** [P] Update logging configuration in `src/VibeGuess.Api/Program.cs`
  - Add structured logging for session lifecycle events
  - Include sessionId and participantId in log context
  - Configure log levels for SignalR and Redis operations

- [ ] **T022** End-to-end integration test in `tests/VibeGuess.Integration.Tests/HostedSessionIntegrationTests.cs`
  - Test complete session flow from creation to completion
  - Test real-time event propagation across connections
  - Test Redis persistence and session cleanup
  - Test host disconnect and reconnection scenarios

- [ ] **T023** Performance and load testing in `tests/VibeGuess.Performance.Tests/HostedSessionLoadTests.cs`
  - Test 100+ concurrent participants across multiple sessions
  - Measure WebSocket message propagation latency (<100ms)
  - Test Redis memory usage and cleanup efficiency
  - Test SignalR backplane scaling across instances

## Dependencies

```
Setup (T001-T002) before Tests (T003-T008)
Tests (T003-T008) before Models (T009-T012)  
Models (T009-T012) before Services (T013-T014)
Services (T013-T014) before Hub (T015)
Hub (T015) before Controller (T016)
All Core before Integration (T017-T018)
Integration before Polish (T019-T023)
```

## Parallel Execution Examples

### Phase 3.2: All Tests in Parallel
```bash
# All test files are independent and can run simultaneously
Task: "REST contract test for POST /api/hosted-sessions"
Task: "REST contract test for GET /api/hosted-sessions/{joinCode}" 
Task: "SignalR Hub integration test for host methods"
Task: "Live session manager unit tests"
```

### Phase 3.3: All Models in Parallel  
```bash
# All model files are independent
Task: "LiveQuizSession model in src/VibeGuess.Core/Models/LiveQuizSession.cs"
Task: "LiveParticipant model in src/VibeGuess.Core/Models/LiveParticipant.cs"
Task: "LiveAnswer model in src/VibeGuess.Core/Models/LiveAnswer.cs"
Task: "SessionSummary entity in src/VibeGuess.Core/Entities/SessionSummary.cs"
```

### Phase 3.8: Polish Tasks in Parallel
```bash
# Independent enhancement tasks
Task: "Connection management and error handling"
Task: "Session cleanup background service" 
Task: "Update logging configuration"
```

## Architecture Notes

### ✅ **In-Memory First**
- All live session data stored in Redis cache with TTL
- NO database persistence for LiveQuizSession, LiveParticipant, LiveAnswer
- Only SessionSummary optionally persisted for analytics

### ✅ **SignalR Primary Interface**
- Real-time gameplay through WebSocket connections
- REST API minimal (only session creation and info)
- Hub handles all game state transitions and scoring

### ✅ **Redis Scaling**
- SignalR backplane for horizontal scaling
- Session state accessible across server instances  
- Automatic cleanup with configurable TTL

## Validation Checklist

- [x] All REST contracts have corresponding tests (T003-T005)
- [x] All SignalR hub methods have integration tests (T006-T007)
- [x] All in-memory models have creation tasks (T009-T012)
- [x] All tests come before implementation (Phase 3.2 → 3.3)
- [x] Parallel tasks are truly independent (different files)
- [x] Each task specifies exact file path
- [x] No live session data persisted to database (Redis only)
- [x] SignalR hub is primary interface for real-time gameplay