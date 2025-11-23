# Implementation Pla**Technical Context**
**Language/Version**: C# 12 on .NET 8.0  
**Primary Dependencies**: ASP.NET Core, SignalR with Redis backplane, Microsoft.Extensions.Caching.Memory, Serilog  
**Storage**: In-memory session state management (Redis/Memory cache), NO database persistence for live sessions, optional final results storage  
**Testing**: xUnit with FluentAssertions; SignalR TestServer for hub integration tests  
**Target Platform**: ASP.NET Core web API hosted on Windows/Linux containers  
**Project Type**: Web backend with real-time SignalR game hub (Option 1 structure)  
**Performance Goals**: Sub-100ms WebSocket message propagation; real-time game state synchronization  
**Constraints**: Host grace disconnect of 30s, ephemeral sessions (no persistence), in-memory participant management  
**Scale/Scope**: 100+ concurrent participants across sessions; horizontal scaling via Redis SignalR backplane and stateless design  
**Architecture**: SignalR Hub as primary interface, minimal REST endpoints, all game logic in memory/cacheStyle Hosted Music Quiz Sessions

**Branch**: `004-host-a-kahoot` | **Date**: 2025-09-28 | **Spec**: `/specs/004-host-a-kahoot/spec.md`  
**Input**: Feature specification from `/specs/004-host-a-kahoot/spec.md`

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
2. Fill Technical Context (detected backend web project with SignalR)
3. Evaluate Constitution Check section below (record existing Repository/UoW usage)
4. Execute Phase 0 → research.md (resolve timing, scoring, moderation unknowns)
5. Execute Phase 1 → contracts, data-model.md, quickstart.md, copilot context
6. Re-evaluate Constitution Check (no new violations)
7. Plan Phase 2 → Describe tasks generation approach (no tasks.md yet)
8. STOP - Ready for /tasks command
```

## Summary
Hosting feature enables an authenticated quiz creator to spin up a live music quiz lobby, distribute a join code, and run gameplay in sync for remote players. Real-time messaging will use ASP.NET Core SignalR, with Redis as the backplane, delivering host controls for pacing, moderation, scoring (100 + time bonus), and analytics snapshots once the session ends.

## Technical Context
**Language/Version**: C# 12 on .NET 8.0  
**Primary Dependencies**: ASP.NET Core, SignalR, Entity Framework Core, Serilog, Redis backplane, AutoMapper  
**Storage**: Existing relational database (SQL Server/PostgreSQL via EF Core) plus Redis for hub scale-out  
**Testing**: xUnit with FluentAssertions; future integration tests through SignalR TestServer harness  
**Target Platform**: ASP.NET Core web API hosted on Windows/Linux containers  
**Project Type**: Web backend (Option 1 structure)  
**Performance Goals**: Sub-200ms propagation for question/leaderboard events; maintain <2s average API latency  
**Constraints**: Host grace disconnect of 30s, configurable question timers (10–120s), blacklist persistence per session  
**Scale/Scope**: At least 100 concurrent participants across sessions; horizontal scaling via Redis backplane and stateless API nodes

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

**Simplicity**:
- Projects: 2 (API + Tests) – within limit
- Using framework directly? Yes, native SignalR hub within API project
- Single data model? Extends existing EF Core entities; new DTOs mirror persistence needs
- Avoiding patterns? Existing Repository/UoW retained (legacy architecture); no new patterns added

**Architecture**:
- EVERY feature as library? Existing API project provides host feature; no new standalone libraries proposed
- Libraries listed: `VibeGuess.Api` (HTTP + SignalR surface), `VibeGuess.Core` (domain), `VibeGuess.Infrastructure` (EF persistence)
- CLI per library: N/A (web backend)
- Library docs: Quickstart added in `/specs/004-host-a-kahoot/quickstart.md`

**Testing (NON-NEGOTIABLE)**:
- RED-GREEN-Refactor cycle enforced via contract and integration tests preceding implementation
- Git commits will stage failing contract/integration tests before feature code
- Order: Contract tests → Hub integration → Unit tests → Implementation
- Real dependencies used? SignalR TestServer with in-memory + Redis backplane integration before deployment
- Integration tests planned for new hub and REST endpoints
- FORBIDDEN actions acknowledged and avoided

**Observability**:
- Structured logging continues via Serilog, extending scope for session lifecycle events
- Host/player events correlated with existing correlation IDs; logs remain unified backend-side
- Context includes sessionId, participantId to aid telemetry

**Versioning**:
- Feature ships under service version `0.1.x`; build number increments on merge
- Breaking changes mitigated by introducing new endpoints without altering existing quiz flows
- Database migrations versioned via EF Core with rollback strategy documented in tasks

## Project Structure
### Documentation (this feature)
```
specs/004-host-a-kahoot/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
│   ├── live-session-openapi.yaml
│   └── live-session-hub.md
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)
```
src/
├── VibeGuess.Api/           # HTTP + SignalR host orchestration
├── VibeGuess.Core/          # Domain entities (new session models)
└── VibeGuess.Infrastructure/ # EF Core mappings & persistence helpers

tests/
├── VibeGuess.Api.Tests/         # Contract + hub integration tests
├── VibeGuess.Integration.Tests/ # End-to-end session validation
└── VibeGuess.Quiz.Tests/        # Extended scoring unit tests
```

**Structure Decision**: Option 1 retained; no frontend/mobile deliverables in this scope.

## Phase 0: Outline & Research
- Unknowns resolved: late join policy, scoring formula, timeout handling, moderation workflow, analytics scope, backplane strategy.
- Research tasks executed (see `/specs/004-host-a-kahoot/research.md`):
  1. Real-time stack alignment with SignalR and Redis backplane
  2. Host disconnect lifecycle and termination timing
  3. Join code management, duplicate name policy, and blacklist reinstatement
  4. Scoring computation and configurable timers
  5. Analytics persistence footprint

**Output**: Research complete with decisions documented; no outstanding “NEEDS CLARIFICATION”.

## Phase 1: Design & Contracts
- Data model updates captured in `/specs/004-host-a-kahoot/data-model.md` (session, participant, answers, analytics).
- REST + hub contracts defined in `/specs/004-host-a-kahoot/contracts/` for session creation, joining, moderation, summary, and SignalR messaging.
- Quickstart walkthrough prepared in `/specs/004-host-a-kahoot/quickstart.md` to validate the feature once implemented.
- Copilot agent context will be updated during implementation via existing script (deferred to coding phase as docs already reference new tech).

## Phase 2: Task Planning Approach
- `/tasks` command will: load plan outputs, enumerate contract endpoints and hub methods, then derive tasks in TDD order.
- Expected ordering:
  1. Author failing contract tests for REST endpoints and hub actions
  2. Add integration tests for session lifecycle (lobby, start, scoring, disconnect)
  3. Implement persistence changes (entities + migrations) followed by application services
  4. Wire up SignalR hub, host controls, and leaderboard broadcasting
  5. Finalize analytics persistence and summary endpoint
- Parallelization: Data model + migration tasks can run alongside hub contract tests ([P] markers to highlight independence).

## Complexity Tracking
| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|---------------------------------------|
| Repository/UoW pattern already in architecture | Consistency with existing VibeGuess data access layer; reusing `IUnitOfWork` keeps changes localized | Replacing with direct DbContext usage would ripple across the codebase and exceed feature scope |

## Progress Tracking
**Phase Status**:
- [x] Phase 0: Research complete (/plan command) ✅
- [x] Phase 1: Design complete (/plan command) ✅
  - [x] Data model updated for in-memory architecture
  - [x] SignalR hub contracts defined
  - [x] Minimal REST API specification
  - [x] Quickstart guide available
- [x] Phase 2: Task planning complete (/plan command - describe approach only) ✅
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete  
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS  
- [x] All NEEDS CLARIFICATION resolved
- [x] Complexity deviations documented
- [x] Architecture aligned with SignalR in-memory requirements

---
*Based on Constitution v2.1.1 - See `/memory/constitution.md`*
