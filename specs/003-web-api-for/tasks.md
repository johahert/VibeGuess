# Tasks: VibeGuess Music Quiz API with Playback Integration

**Input**: Design documents from `/specs/003-web-api-for/`
**Prerequisites**: plan.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

## Execution Flow (main)
```
1. Load plan.md from feature directory
   → ✅ Loaded: .NET 8 Web API with modular architecture
   → Tech stack: Entity Framework Core, AutoMapper, FluentValidation, Polly, MediatR, Serilog
   → Structure: Single project with 4 modules (Core, Auth, Quiz, Playback)
2. Load design documents:
   → data-model.md: 10 entities (User, Quiz, Question, AnswerOption, TrackMetadata, etc.)
   → contracts/: 4 API contract files (auth, quiz, playback, health)
   → research.md: Technology decisions and integration patterns
3. Generate tasks by category:
   → Setup: .NET project init, NuGet packages, project structure
   → Tests: Contract tests (25+ endpoints), integration tests
   → Core: Entity models, services, repositories
   → Integration: DbContext, middleware, external APIs
   → Polish: Unit tests, performance validation, documentation
4. Apply TDD rules:
   → All tests before implementation (RED-GREEN-REFACTOR)
   → Contract tests per endpoint [P]
   → Integration tests per user story [P]
   → Different projects/files = parallel [P]
5. Number tasks sequentially (T001-T065)
6. Generate dependency graph for execution order
7. Create parallel execution examples for efficient development
```

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **File paths**: Absolute paths based on project structure from plan.md

## Phase 3.1: Project Setup & Infrastructure

### T001 - Initialize Solution Structure
Create .NET 8 solution with modular architecture per plan.md
- **Files**: `src/VibeGuess.sln`, project directories
- **Dependencies**: None
- **Validation**: Solution builds successfully

### T002 - [P] Core Project Setup  
Initialize VibeGuess.Core project with shared entities and interfaces
- **Files**: `src/VibeGuess.Core/VibeGuess.Core.csproj`
- **Packages**: None (pure POCO classes)
- **Dependencies**: T001

### T003 - [P] API Project Setup
Initialize main Web API project with dependencies
- **Files**: `src/VibeGuess.Api/VibeGuess.Api.csproj`, `Program.cs`
- **Packages**: ASP.NET Core 8.0, Entity Framework Core, Serilog, AutoMapper
- **Dependencies**: T001

### T004 - [P] Spotify Auth Module Setup
Initialize Spotify authentication module
- **Files**: `src/VibeGuess.Spotify.Authentication/VibeGuess.Spotify.Authentication.csproj`
- **Packages**: Microsoft.AspNetCore.Authentication.JwtBearer
- **Dependencies**: T001

### T005 - [P] Quiz Generation Module Setup  
Initialize AI quiz generation module
- **Files**: `src/VibeGuess.Quiz.Generation/VibeGuess.Quiz.Generation.csproj`
- **Packages**: OpenAI .NET SDK, FluentValidation
- **Dependencies**: T001

### T006 - [P] Spotify Playback Module Setup
Initialize Spotify playback control module  
- **Files**: `src/VibeGuess.Spotify.Playback/VibeGuess.Spotify.Playback.csproj`
- **Packages**: Polly for resilience patterns
- **Dependencies**: T001

### T007 - [P] Infrastructure Project Setup
Initialize infrastructure project for data access
- **Files**: `src/VibeGuess.Infrastructure/VibeGuess.Infrastructure.csproj`
- **Packages**: Entity Framework Core, SQL Server provider, Testcontainers
- **Dependencies**: T001

### T008 - [P] Test Projects Setup
Initialize all test projects with testing frameworks
- **Files**: `tests/VibeGuess.*.Tests/*.csproj` (5 test projects)
- **Packages**: xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing, Testcontainers
- **Dependencies**: T001

## Phase 3.2: Contract Tests (TDD - MUST FAIL FIRST) ⚠️

### Authentication API Contract Tests

### T009 - [P] Auth Login Contract Test
Contract test for POST /api/auth/spotify/login endpoint
- **File**: `tests/VibeGuess.Api.Tests/Contracts/AuthLoginContractTests.cs`
- **Test**: Validate request/response schema per auth-api.md
- **Must Fail**: No implementation exists yet

### T010 - [P] Auth Callback Contract Test  
Contract test for POST /api/auth/spotify/callback endpoint
- **File**: `tests/VibeGuess.Api.Tests/Contracts/AuthCallbackContractTests.cs`
- **Test**: OAuth callback flow validation per auth-api.md
- **Must Fail**: No implementation exists yet

### T011 - [P] Auth Refresh Contract Test
Contract test for POST /api/auth/refresh endpoint  
- **File**: `tests/VibeGuess.Api.Tests/Contracts/AuthRefreshContractTests.cs`
- **Test**: Token refresh validation per auth-api.md
- **Must Fail**: No implementation exists yet

### T012 - [P] Auth Profile Contract Test
Contract test for GET /api/auth/me endpoint
- **File**: `tests/VibeGuess.Api.Tests/Contracts/AuthProfileContractTests.cs`  
- **Test**: User profile response validation per auth-api.md
- **Must Fail**: No implementation exists yet

### Quiz API Contract Tests

### T013 - [P] Quiz Generation Contract Test
Contract test for POST /api/quiz/generate endpoint
- **File**: `tests/VibeGuess.Api.Tests/Contracts/QuizGenerationContractTests.cs`
- **Test**: Quiz generation request/response per quiz-api.md
- **Must Fail**: No implementation exists yet

### T014 - [P] Quiz Retrieval Contract Test  
Contract test for GET /api/quiz/{id} endpoint
- **File**: `tests/VibeGuess.Api.Tests/Contracts/QuizRetrievalContractTests.cs`
- **Test**: Quiz data structure validation per quiz-api.md
- **Must Fail**: No implementation exists yet

### T015 - [P] Quiz Session Contract Test
Contract test for POST /api/quiz/{id}/start-session endpoint
- **File**: `tests/VibeGuess.Api.Tests/Contracts/QuizSessionContractTests.cs`
- **Test**: Session creation validation per quiz-api.md
- **Must Fail**: No implementation exists yet

### T016 - [P] Quiz History Contract Test
Contract test for GET /api/quiz/my-quizzes endpoint  
- **File**: `tests/VibeGuess.Api.Tests/Contracts/QuizHistoryContractTests.cs`
- **Test**: Pagination and filtering per quiz-api.md
- **Must Fail**: No implementation exists yet

### Playback API Contract Tests

### T017 - [P] Device List Contract Test
Contract test for GET /api/playback/devices endpoint
- **File**: `tests/VibeGuess.Api.Tests/Contracts/DeviceListContractTests.cs`
- **Test**: Device data structure per playback-api.md
- **Must Fail**: No implementation exists yet

### T018 - [P] Play Track Contract Test
Contract test for POST /api/playback/play endpoint  
- **File**: `tests/VibeGuess.Api.Tests/Contracts/PlayTrackContractTests.cs`
- **Test**: Playback control validation per playback-api.md
- **Must Fail**: No implementation exists yet

### T019 - [P] Pause Control Contract Test
Contract test for POST /api/playback/pause endpoint
- **File**: `tests/VibeGuess.Api.Tests/Contracts/PauseControlContractTests.cs`
- **Test**: Pause functionality per playback-api.md
- **Must Fail**: No implementation exists yet

### T020 - [P] Playback Status Contract Test
Contract test for GET /api/playback/status endpoint
- **File**: `tests/VibeGuess.Api.Tests/Contracts/PlaybackStatusContractTests.cs`  
- **Test**: Status response structure per playback-api.md
- **Must Fail**: No implementation exists yet

### Health API Contract Tests

### T021 - [P] Health Check Contract Test  
Contract test for GET /api/health endpoint
- **File**: `tests/VibeGuess.Api.Tests/Contracts/HealthCheckContractTests.cs`
- **Test**: Basic health response per health-api.md
- **Must Fail**: No implementation exists yet

### T022 - [P] Test Endpoints Contract Test
Contract test for POST /api/health/test/* endpoints
- **File**: `tests/VibeGuess.Api.Tests/Contracts/TestEndpointsContractTests.cs`
- **Test**: Testing endpoint validation per health-api.md  
- **Must Fail**: No implementation exists yet

## Phase 3.3: Integration Tests (TDD - MUST FAIL FIRST) ⚠️

### T023 - [P] Complete Quiz Workflow Integration Test
End-to-end test for complete quiz generation and playback workflow per quickstart.md
- **File**: `tests/VibeGuess.Integration.Tests/CompleteWorkflowTests.cs`
- **Test**: Auth → Generate → Session → Playback sequence
- **Must Fail**: No services implemented yet

### T024 - [P] Spotify Authentication Integration Test  
Integration test for OAuth 2.0 PKCE flow with Spotify
- **File**: `tests/VibeGuess.Spotify.Tests/AuthenticationIntegrationTests.cs`
- **Test**: Real Spotify OAuth flow (with test credentials)
- **Must Fail**: No auth service implemented yet

### T025 - [P] AI Quiz Generation Integration Test
Integration test for OpenAI API quiz generation 
- **File**: `tests/VibeGuess.Quiz.Tests/GenerationIntegrationTests.cs`
- **Test**: Real OpenAI API calls with prompt validation
- **Must Fail**: No generation service implemented yet

### T026 - [P] Spotify Playback Integration Test
Integration test for Spotify Web API playback control
- **File**: `tests/VibeGuess.Spotify.Tests/PlaybackIntegrationTests.cs`  
- **Test**: Real device control and track playback
- **Must Fail**: No playback service implemented yet

### T027 - [P] Database Operations Integration Test
Integration test for Entity Framework operations with Testcontainers
- **File**: `tests/VibeGuess.Infrastructure.Tests/DatabaseIntegrationTests.cs`
- **Test**: CRUD operations on all entities with real SQL Server
- **Must Fail**: No DbContext implemented yet

## Phase 3.4: Core Entity Models (After Tests Fail)

### T028 - [P] User Entity Model
Implement User entity with validation per data-model.md
- **File**: `src/VibeGuess.Core/Entities/User.cs`
- **Dependencies**: T002, T009-T027 (tests must fail first)
- **Validation**: Properties, navigation properties, validation attributes

### T029 - [P] Quiz Entity Model  
Implement Quiz entity with enums per data-model.md
- **File**: `src/VibeGuess.Core/Entities/Quiz.cs`  
- **Dependencies**: T002, T009-T027 (tests must fail first)
- **Validation**: Format/Difficulty/Status enums, relationships

### T030 - [P] Question Entity Model
Implement Question entity with answer options per data-model.md
- **File**: `src/VibeGuess.Core/Entities/Question.cs`
- **Dependencies**: T002, T009-T027 (tests must fail first)  
- **Validation**: Question types, track relationships

### T031 - [P] Answer Option Entity Model
Implement AnswerOption entity per data-model.md
- **File**: `src/VibeGuess.Core/Entities/AnswerOption.cs`
- **Dependencies**: T002, T009-T027 (tests must fail first)
- **Validation**: Multiple choice structure

### T032 - [P] Track Metadata Entity Model
Implement TrackMetadata entity for Spotify tracks per data-model.md  
- **File**: `src/VibeGuess.Core/Entities/TrackMetadata.cs`
- **Dependencies**: T002, T009-T027 (tests must fail first)
- **Validation**: Spotify ID format, market availability

### T033 - [P] Quiz Session Entity Model
Implement QuizSession entity for active sessions per data-model.md
- **File**: `src/VibeGuess.Core/Entities/QuizSession.cs`
- **Dependencies**: T002, T009-T027 (tests must fail first)
- **Validation**: Session status, progress tracking

### T034 - [P] User Answer Entity Model  
Implement UserAnswer entity for quiz responses per data-model.md
- **File**: `src/VibeGuess.Core/Entities/UserAnswer.cs`
- **Dependencies**: T002, T009-T027 (tests must fail first)
- **Validation**: Answer validation, scoring

### T035 - [P] Device Info Entity Model
Implement DeviceInfo entity for Spotify devices per data-model.md
- **File**: `src/VibeGuess.Core/Entities/DeviceInfo.cs`
- **Dependencies**: T002, T009-T027 (tests must fail first)
- **Validation**: Device types, restrictions

### T036 - [P] Playback Event Entity Model
Implement PlaybackEvent entity for playback history per data-model.md
- **File**: `src/VibeGuess.Core/Entities/PlaybackEvent.cs` 
- **Dependencies**: T002, T009-T027 (tests must fail first)
- **Validation**: Event types, success tracking

### T037 - [P] User Settings Entity Model
Implement UserSettings entity for preferences per data-model.md
- **File**: `src/VibeGuess.Core/Entities/UserSettings.cs`
- **Dependencies**: T002, T009-T027 (tests must fail first)
- **Validation**: Default values, language codes

## Phase 3.5: Data Access Layer

### T038 - DbContext Implementation  
Implement VibeGuessDbContext with all entities per data-model.md
- **File**: `src/VibeGuess.Infrastructure/Data/VibeGuessDbContext.cs`
- **Dependencies**: T007, T028-T037 (all entities)
- **Validation**: Relationships, indexes, query filters

### T039 - Entity Framework Configurations
Implement entity configurations for complex relationships per data-model.md
- **File**: `src/VibeGuess.Infrastructure/Data/Configurations/*.cs` (10 files)
- **Dependencies**: T038
- **Validation**: Foreign keys, indexes, constraints

### T040 - Database Migrations
Create initial database migration with all entities  
- **File**: `src/VibeGuess.Infrastructure/Migrations/InitialCreate.cs`
- **Dependencies**: T038, T039
- **Validation**: Migration generates correct schema

### T041 - [P] Repository Interfaces
Define repository interfaces for external API abstraction per research.md
- **File**: `src/VibeGuess.Core/Interfaces/ISpotifyRepository.cs`, `IQuizRepository.cs`, etc.
- **Dependencies**: T002, T028-T037
- **Validation**: SOLID principles, testability

### T042 - Repository Implementations  
Implement repositories for database operations per data-model.md
- **File**: `src/VibeGuess.Infrastructure/Repositories/*.cs`
- **Dependencies**: T038, T041
- **Validation**: CRUD operations, query optimization

## Phase 3.6: External Service Integration

### T043 - [P] Spotify Authentication Service
Implement OAuth 2.0 PKCE flow per research.md and auth-api.md
- **File**: `src/VibeGuess.Spotify.Authentication/Services/SpotifyAuthService.cs`
- **Dependencies**: T004, T041
- **Validation**: PKCE flow, token management, rate limiting

### T044 - [P] Spotify Web API Client
Implement Spotify Web API integration with Polly resilience per research.md  
- **File**: `src/VibeGuess.Spotify.Playback/Services/SpotifyApiClient.cs`
- **Dependencies**: T006, T041
- **Validation**: Rate limiting, error handling, circuit breaker

### T045 - [P] OpenAI Integration Service
Implement AI quiz generation with OpenAI API per research.md
- **File**: `src/VibeGuess.Quiz.Generation/Services/OpenAIQuizService.cs`
- **Dependencies**: T005, T041  
- **Validation**: Prompt templates, content filtering, cost monitoring

### T046 - [P] JWT Token Service
Implement JWT token generation and validation per research.md
- **File**: `src/VibeGuess.Spotify.Authentication/Services/JwtTokenService.cs`
- **Dependencies**: T004, T043
- **Validation**: Token security, expiration, refresh logic

## Phase 3.7: Business Logic Services  

### T047 - Quiz Generation Service
Implement quiz generation orchestration per quiz-api.md
- **File**: `src/VibeGuess.Quiz.Generation/Services/QuizGenerationService.cs`
- **Dependencies**: T045, T044, T042
- **Validation**: Prompt processing, track validation, question assembly

### T048 - Playback Control Service
Implement Spotify playback control per playback-api.md
- **File**: `src/VibeGuess.Spotify.Playback/Services/PlaybackControlService.cs`  
- **Dependencies**: T044, T042
- **Validation**: Device management, playback commands, error recovery

### T049 - Quiz Session Service
Implement quiz session management per quiz-api.md
- **File**: `src/VibeGuess.Api/Services/QuizSessionService.cs`
- **Dependencies**: T042, T047, T048
- **Validation**: Session lifecycle, progress tracking, scoring

### T050 - User Management Service
Implement user profile and settings management per auth-api.md
- **File**: `src/VibeGuess.Api/Services/UserManagementService.cs`
- **Dependencies**: T043, T042
- **Validation**: Profile updates, settings persistence, data validation

## Phase 3.8: API Controllers

### T051 - Authentication Controller  
Implement auth endpoints per auth-api.md contracts
- **File**: `src/VibeGuess.Api/Controllers/AuthController.cs`
- **Dependencies**: T043, T046, T050
- **Validation**: All auth contract tests pass (T009-T012)

### T052 - Quiz Controller
Implement quiz endpoints per quiz-api.md contracts  
- **File**: `src/VibeGuess.Api/Controllers/QuizController.cs`
- **Dependencies**: T047, T049
- **Validation**: All quiz contract tests pass (T013-T016)

### T053 - Playback Controller
Implement playback endpoints per playback-api.md contracts
- **File**: `src/VibeGuess.Api/Controllers/PlaybackController.cs`
- **Dependencies**: T048, T049
- **Validation**: All playback contract tests pass (T017-T020)

### T054 - Health Controller
Implement health and testing endpoints per health-api.md contracts
- **File**: `src/VibeGuess.Api/Controllers/HealthController.cs` 
- **Dependencies**: T043, T047, T048
- **Validation**: All health contract tests pass (T021-T022)

## Phase 3.9: API Infrastructure

### T055 - Dependency Injection Configuration
Configure all services in DI container per research.md  
- **File**: `src/VibeGuess.Api/Extensions/ServiceCollectionExtensions.cs`
- **Dependencies**: T003, T043-T054
- **Validation**: All services resolve correctly, correct lifetimes

### T056 - Middleware Pipeline
Implement authentication, logging, error handling middleware per research.md
- **File**: `src/VibeGuess.Api/Middleware/*.cs`
- **Dependencies**: T003, T046
- **Validation**: HTTPS, CORS, rate limiting, security headers

### T057 - Configuration Management  
Implement configuration for all external services per research.md
- **File**: `src/VibeGuess.Api/appsettings.json`, `appsettings.Development.json`  
- **Dependencies**: T003, T043-T048
- **Validation**: Environment-specific configs, secrets management

### T058 - OpenAPI Documentation
Generate Swagger/OpenAPI documentation per all contract files
- **File**: `src/VibeGuess.Api/Extensions/SwaggerExtensions.cs`
- **Dependencies**: T051-T054
- **Validation**: All endpoints documented, example responses

## Phase 3.10: Integration & Validation

### T059 - Integration Test Validation
Verify all integration tests pass with implemented services  
- **Files**: Tests from T023-T027
- **Dependencies**: T051-T058 (full implementation)
- **Validation**: All integration scenarios work end-to-end

### T060 - Contract Test Validation  
Verify all contract tests pass with implemented endpoints
- **Files**: Tests from T009-T022  
- **Dependencies**: T051-T054 (controllers implemented)
- **Validation**: All API contracts satisfied

### T061 - Database Schema Validation
Verify database schema matches data model requirements
- **Dependencies**: T038-T040, T059
- **Validation**: Migrations work, constraints enforced, performance acceptable

## Phase 3.11: Performance & Polish

### T062 - [P] Unit Test Coverage
Achieve 80%+ test coverage for all business logic per constitution
- **Files**: `tests/VibeGuess.*.Tests/Unit/*.cs` (multiple files)  
- **Dependencies**: T047-T050 (services implemented)
- **Validation**: Coverage reports, fast execution

### T063 - [P] Performance Optimization
Optimize for constitutional performance targets per research.md
- **Files**: Various service files for caching, async patterns
- **Dependencies**: T047-T050, T055
- **Validation**: <5s quiz generation, <2s playback control, 100+ concurrent users

### T064 - [P] Security Hardening
Implement security measures per constitutional requirements
- **Files**: Various middleware and service files
- **Dependencies**: T055-T057  
- **Validation**: Input validation, rate limiting, audit logging

### T065 - Quickstart Validation
Execute quickstart.md scenarios to verify complete functionality  
- **File**: Manual execution of quickstart.md examples
- **Dependencies**: T059-T064 (everything implemented and polished)
- **Validation**: All quickstart scenarios work correctly

## Dependencies

### Critical Path
1. **Setup** (T001-T008) → **Contract Tests** (T009-T022) → **Integration Tests** (T023-T027)
2. **Entity Models** (T028-T037) → **Data Access** (T038-T042) → **Services** (T043-T050)
3. **Controllers** (T051-T054) → **Infrastructure** (T055-T058) → **Validation** (T059-T061)
4. **Performance & Polish** (T062-T065)

### Parallel Execution Groups

#### Group A - Project Setup (Can run simultaneously)
```
Task: T002 "Core Project Setup" 
Task: T003 "API Project Setup"
Task: T004 "Spotify Auth Module Setup"
Task: T005 "Quiz Generation Module Setup" 
Task: T006 "Spotify Playback Module Setup"
Task: T007 "Infrastructure Project Setup"
Task: T008 "Test Projects Setup"
```

#### Group B - Contract Tests (Must fail before implementation)
```
Task: T009 "Auth Login Contract Test"
Task: T010 "Auth Callback Contract Test"  
Task: T011 "Auth Refresh Contract Test"
Task: T012 "Auth Profile Contract Test"
Task: T013 "Quiz Generation Contract Test"
Task: T014 "Quiz Retrieval Contract Test"
Task: T015 "Quiz Session Contract Test"
Task: T016 "Quiz History Contract Test"
Task: T017 "Device List Contract Test"
Task: T018 "Play Track Contract Test"
Task: T019 "Pause Control Contract Test" 
Task: T020 "Playback Status Contract Test"
Task: T021 "Health Check Contract Test"
Task: T022 "Test Endpoints Contract Test"
```

#### Group C - Integration Tests (Must fail before implementation)  
```
Task: T023 "Complete Quiz Workflow Integration Test"
Task: T024 "Spotify Authentication Integration Test"
Task: T025 "AI Quiz Generation Integration Test"
Task: T026 "Spotify Playback Integration Test"
Task: T027 "Database Operations Integration Test"
```

#### Group D - Entity Models (After tests fail)
```
Task: T028 "User Entity Model"
Task: T029 "Quiz Entity Model"
Task: T030 "Question Entity Model" 
Task: T031 "Answer Option Entity Model"
Task: T032 "Track Metadata Entity Model"
Task: T033 "Quiz Session Entity Model"
Task: T034 "User Answer Entity Model"
Task: T035 "Device Info Entity Model"
Task: T036 "Playback Event Entity Model"
Task: T037 "User Settings Entity Model"
```

## Validation Checklist
*GATE: All must pass before considering tasks complete*

- [x] All contract files have corresponding contract tests (auth, quiz, playback, health)
- [x] All entities from data-model.md have model creation tasks  
- [x] All tests come before implementation (TDD enforced)
- [x] Parallel tasks are truly independent (different files/modules)
- [x] Each task specifies exact file path for implementation
- [x] Dependencies prevent implementation before tests fail
- [x] Performance and security requirements addressed
- [x] Integration scenarios from quickstart.md covered

## Notes

### TDD Enforcement
- **RED phase**: Contract and integration tests (T009-T027) MUST be written first and MUST FAIL
- **GREEN phase**: Implementation (T028-T058) to make tests pass with minimal code
- **REFACTOR phase**: Polish and optimization (T062-T064)

### Constitutional Compliance  
- **Testing**: 80%+ coverage achieved through comprehensive test tasks
- **Security**: Authentication, input validation, rate limiting implemented
- **Performance**: Specific optimization tasks for constitutional targets
- **Observability**: Structured logging and monitoring throughout

### Modular Architecture Benefits
- **Parallel Development**: Different modules can be developed simultaneously
- **Testing Isolation**: Each module has dedicated test projects  
- **Separation of Concerns**: Clear boundaries between auth, quiz generation, and playback
- **Maintainability**: Independent versioning and deployment of modules

---

**🎯 Ready for execution! All tasks are specific, dependency-ordered, and follow TDD principles.**