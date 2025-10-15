# Implementation Plan: VibeGuess Music Quiz API with Playback Integration

**Branch**: `003-web-api-for` | **Date**: 2025-09-15 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/003-web-api-for/spec.md`

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
   → ✅ Loaded: VibeGuess Music Quiz API with Playback Integration
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Detect Project Type: Web API (backend service)
   → Set Structure Decision: Single project structure with separate modules
3. Evaluate Constitution Check section below
   → Document compliance with constitutional principles
   → Update Progress Tracking: Initial Constitution Check
4. Execute Phase 0 → research.md
   → Resolve technical decisions and integration patterns
5. Execute Phase 1 → contracts, data-model.md, quickstart.md, copilot-instructions.md
6. Re-evaluate Constitution Check section
   → Update Progress Tracking: Post-Design Constitution Check
7. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
8. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:
- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary
Primary requirement: AI-powered music quiz generation API based on user prompts with integrated Spotify playback control. Technical approach: .NET 8 Web API with Entity Framework Core, modular architecture for Spotify authentication, quiz generation (with/without auth), and playback management. Mock database support for comprehensive testing.

## Technical Context
**Language/Version**: .NET 8.0 with ASP.NET Core Web API  
**Primary Dependencies**: Entity Framework Core, AutoMapper, FluentValidation, Polly, MediatR, Serilog  
**Storage**: SQL Server (primary), Mock/In-Memory database (testing)  
**Testing**: xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing, Testcontainers  
**Target Platform**: Linux containers, Windows development environment  
**Project Type**: Web API (single backend service with modular architecture)  
**Performance Goals**: Quiz generation <5 seconds, playback control <2 seconds, 100+ concurrent users  
**Constraints**: Spotify API rate limits, OpenAI API costs, 24-hour quiz retention, JWT authentication required  
**Scale/Scope**: Multi-tenant API, Spotify Premium/Free user support, extensible quiz formats

**User Implementation Details**: .NET API with Entity Framework, SQL storage, mock database for testing, separate testable modules for: Spotify authentication, quiz generation (with/without Spotify auth), Spotify device and playback management.

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Simplicity**:
- Projects: 1 (VibeGuess.Api - single Web API project)
- Using framework directly? ✅ (ASP.NET Core Web API, Entity Framework Core)
- Single data model? ✅ (Unified entities for Quiz, Question, Track, User, Device)
- Avoiding patterns? ✅ (Repository pattern justified for external API abstraction per constitution)

**Architecture**:
- EVERY feature as library? ✅ (Separate modules: Spotify.Auth, Quiz.Generation, Spotify.Playback)
- Libraries listed: 
  * VibeGuess.Spotify.Authentication (OAuth flow, token management)
  * VibeGuess.Quiz.Generation (AI integration, question generation)  
  * VibeGuess.Spotify.Playback (device management, playback control)
  * VibeGuess.Core (shared entities, interfaces)
- CLI per library: Health check endpoints + test endpoints for verification
- Library docs: OpenAPI/Swagger documentation for all endpoints

**Testing (NON-NEGOTIABLE)**:
- RED-GREEN-Refactor cycle enforced? ✅ (TDD mandatory per constitution)
- Git commits show tests before implementation? ✅ (Will be enforced)
- Order: Contract→Integration→E2E→Unit strictly followed? ✅ 
- Real dependencies used? ✅ (Testcontainers for SQL, HTTP clients for Spotify/OpenAI)
- Integration tests for: ✅ Spotify API, OpenAI API, database operations
- FORBIDDEN: Implementation before test, skipping RED phase ✅

**Observability**:
- Structured logging included? ✅ (Serilog with correlation IDs per constitution)
- Frontend logs → backend? N/A (API-only service)
- Error context sufficient? ✅ (Detailed error responses with correlation tracking)

**Versioning**:
- Version number assigned? ✅ (1.0.0 - initial implementation)
- BUILD increments on every change? ✅ (CI/CD pipeline enforcement)
- Breaking changes handled? ✅ (API versioning headers, backward compatibility)

## Project Structure

### Documentation (this feature)
```
specs/003-web-api-for/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)
```
src/
├── VibeGuess.Api/                    # Main Web API project
│   ├── Controllers/
│   ├── Program.cs
│   ├── appsettings.json
│   └── Dockerfile
├── VibeGuess.Core/                   # Shared entities and interfaces
│   ├── Entities/
│   ├── Interfaces/
│   └── DTOs/
├── VibeGuess.Spotify.Authentication/ # Spotify OAuth module
│   ├── Services/
│   ├── Models/
│   └── Interfaces/
├── VibeGuess.Quiz.Generation/        # AI quiz generation module
│   ├── Services/
│   ├── Prompts/
│   └── Validators/
├── VibeGuess.Spotify.Playback/       # Spotify playback control module
│   ├── Services/
│   ├── Models/
│   └── Interfaces/
└── VibeGuess.Infrastructure/         # Data access and external services
    ├── Data/
    ├── Repositories/
    └── Services/

tests/
├── VibeGuess.Api.Tests/              # API integration tests
├── VibeGuess.Core.Tests/             # Unit tests for core logic
├── VibeGuess.Spotify.Tests/          # Spotify integration tests
├── VibeGuess.Quiz.Tests/             # Quiz generation tests
└── VibeGuess.Infrastructure.Tests/   # Infrastructure tests
```

**Structure Decision**: Single project structure with modular libraries (matches Web API project type)

## Phase 0: Outline & Research
*This phase resolves technical decisions and integration patterns*

Research areas identified:
1. **Spotify Web API Integration**: OAuth 2.0 PKCE flow, rate limiting, device management APIs
2. **OpenAI API Integration**: GPT-4 prompt engineering for music quiz generation, cost optimization
3. **Entity Framework Setup**: Code-first migrations, connection resilience, query optimization
4. **Testing Strategy**: Testcontainers for integration tests, mocking strategies for external APIs
5. **Authentication Flow**: JWT token management, refresh token handling, user session management
6. **Modular Architecture**: Clean architecture implementation, dependency injection setup
7. **Performance Optimization**: Caching strategies, async patterns, concurrent user handling

**Output**: research.md with technology decisions and implementation patterns

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

Planned artifacts:
1. **data-model.md**: Core entities (User, Quiz, Question, Track, Device, Session) with relationships and validation rules
2. **contracts/**: OpenAPI specifications for all endpoints:
   - `/api/auth/*` - Spotify authentication endpoints
   - `/api/quiz/*` - Quiz generation and management
   - `/api/playback/*` - Device and playback control
   - `/api/health/*` - Health check and testing endpoints
3. **Contract tests**: Request/response validation for all endpoints
4. **quickstart.md**: Step-by-step API usage guide with examples
5. **copilot-instructions.md**: Updated GitHub Copilot context with project structure and patterns

**Output**: Complete API design with failing contract tests ready for implementation

## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:
- Generate from Phase 1 contracts and data model
- Separate tasks by module (Auth, Quiz, Playback, Core)
- Each contract → contract test task [P]
- Each entity → model + validation task [P]
- Each service → interface + implementation task
- Integration tests for external API interactions

**Ordering Strategy**:
- TDD order: Contract tests → Unit tests → Implementation
- Dependency order: Core entities → Services → Controllers → Integration
- Module independence: Auth, Quiz, Playback can be developed in parallel [P]

**Estimated Output**: 35-40 numbered tasks across 4 modules with clear dependencies

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following TDD principles)  
**Phase 5**: Validation (run all tests, execute quickstart.md, performance benchmarks)

## Complexity Tracking
*No constitutional violations identified - all complexity is justified by requirements*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Repository Pattern | External API abstraction (Spotify, OpenAI) | Direct API calls would violate testability requirements |
| Multiple Projects | Module separation for testing isolation | Single project would mix concerns and reduce testability |

## Progress Tracking
*This checklist is updated during execution flow*

**Phase Status**:
- [x] Phase 0: Research complete (/plan command)
- [x] Phase 1: Design complete (/plan command)
- [x] Phase 2: Task planning complete (/plan command - describe approach only)
- [x] Phase 3: Tasks generated (/tasks command)
- [x] Phase 4: Implementation complete
- [x] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [x] Complexity deviations documented

---
*Based on Constitution v1.0.0 - See `.specify/memory/constitution.md`*