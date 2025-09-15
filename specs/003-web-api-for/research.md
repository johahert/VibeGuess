# Research & Technology Decisions

**Feature**: VibeGuess Music Quiz API with Playback Integration  
**Date**: 2025-09-15  
**Status**: Research Complete

## Technology Stack Decisions

### Core Platform
**Decision**: .NET 8.0 with ASP.NET Core Web API  
**Rationale**: 
- Constitutional requirement for .NET 8.0+
- Mature ecosystem for API development
- Strong performance characteristics for concurrent users
- Excellent testing framework integration
- Native support for async/await patterns required for external API calls

**Alternatives considered**: 
- .NET 6.0 (rejected - not latest LTS)
- Node.js/Express (rejected - not aligned with constitution)

### Database & ORM
**Decision**: Entity Framework Core with SQL Server (production), In-Memory provider (testing)  
**Rationale**:
- Constitutional requirement for Entity Framework Core
- Code-first migrations support rapid development
- Strong connection resilience features for cloud deployment
- In-Memory provider enables fast, isolated unit tests
- Query optimization tools for performance requirements

**Alternatives considered**:
- Dapper (rejected - constitution specifies EF Core)
- PostgreSQL (acceptable alternative, SQL Server chosen for Windows dev environment)

### External API Integration

#### Spotify Web API
**Decision**: HttpClient with Polly for resilience  
**Rationale**:
- OAuth 2.0 PKCE flow support as required by constitution
- Built-in retry policies for rate limit handling (429 responses)
- Circuit breaker pattern for service failures
- Configurable timeout handling for performance targets

**Integration Pattern**:
```
Spotify API Rate Limits: 100 requests/minute/application
- Implement exponential backoff on 429 responses
- Cache user profile data for session duration
- Batch track validation requests where possible
```

#### OpenAI API Integration
**Decision**: Official OpenAI .NET client with custom prompt templates  
**Rationale**:
- Official SDK provides best practices and updates
- Built-in retry and error handling
- Cost tracking capabilities for budget monitoring
- Template system for consistent prompt engineering

**Prompt Strategy**:
```
Base Template: "Generate music quiz questions based on: {userPrompt}"
- Include format specifications (multiple choice, free text)
- Request track recommendations with Spotify IDs
- Implement content filtering for appropriateness
- Cost optimization through prompt length management
```

### Architecture Patterns

#### Clean Architecture Implementation
**Decision**: Modular architecture with clear separation of concerns  
**Rationale**:
- Constitutional requirement for SOLID principles
- Enables independent testing of each module
- Supports parallel development of features
- Facilitates future extensibility (new quiz formats)

**Module Structure**:
```
Core: Entities, Interfaces, DTOs (dependency-free)
Application: Business logic, validation, orchestration
Infrastructure: External APIs, database, file I/O
Presentation: Controllers, middleware, routing
```

#### Dependency Injection
**Decision**: Built-in ASP.NET Core DI container  
**Rationale**:
- No additional dependencies required
- Supports lifetime management (Singleton, Scoped, Transient)
- Integration with configuration system
- Testing-friendly service registration

### Authentication & Security

#### JWT Token Management
**Decision**: Microsoft.AspNetCore.Authentication.JwtBearer with custom token refresh  
**Rationale**:
- Stateless authentication for horizontal scaling
- Integration with Spotify OAuth flow
- Automatic token validation and expiry handling
- Support for refresh token rotation

**Security Implementation**:
```
- HTTPS only in production (TLS 1.2+)
- JWT tokens with 1-hour expiry, 30-day refresh
- Rate limiting per user/IP address
- Input validation on all endpoints
- SQL injection prevention through parameterized queries
```

### Testing Strategy

#### Unit Testing
**Decision**: xUnit with Moq for mocking  
**Rationale**:
- Excellent async test support
- Fluent assertion syntax
- Strong mocking capabilities for external dependencies
- Integration with .NET CLI and CI/CD

#### Integration Testing
**Decision**: Microsoft.AspNetCore.Mvc.Testing with Testcontainers  
**Rationale**:
- Real database testing with Docker containers
- Isolated test environments per test class
- Actual HTTP client testing of APIs
- Support for testing external API interactions

**Test Database Strategy**:
```
- Testcontainers SQL Server for integration tests
- In-Memory provider for fast unit tests
- Database migrations tested in container environment
- Test data seeding for consistent scenarios
```

### Performance & Scalability

#### Caching Strategy
**Decision**: Redis for distributed caching, Memory cache for single instance  
**Rationale**:
- Constitutional requirement for Redis
- Session data sharing across instances
- Quiz result caching (24-hour retention)
- User profile and token caching

**Cache Keys**:
```
user:{userId}:profile - User Spotify profile data
quiz:{quizId} - Generated quiz results
device:{userId}:selection - Selected playback device
rate-limit:{userId} - API call tracking
```

#### Async Patterns
**Decision**: Full async/await implementation with ConfigureAwait(false)  
**Rationale**:
- Constitutional requirement for async I/O operations
- Non-blocking external API calls
- Better resource utilization for concurrent users
- Improved response times under load

### Monitoring & Observability

#### Logging
**Decision**: Serilog with structured logging  
**Rationale**:
- Constitutional requirement for Serilog
- Correlation ID tracking across services
- Integration with Application Insights
- Performance metric collection

**Log Structure**:
```
{
  "timestamp": "2025-09-15T10:00:00Z",
  "level": "Information",
  "correlationId": "abc-123",
  "userId": "spotify-user-id",
  "operation": "GenerateQuiz",
  "duration": "3.2s",
  "properties": { ... }
}
```

#### Health Checks
**Decision**: ASP.NET Core Health Checks with custom probes  
**Rationale**:
- Built-in framework support
- Dependency health monitoring (Spotify, OpenAI, Database)
- Kubernetes readiness/liveness probe support
- Detailed component status reporting

## Integration Patterns

### Spotify API Integration
**Rate Limiting Compliance**:
- Implement sliding window rate limiter
- Respect 429 Retry-After headers
- Graceful degradation when limits exceeded
- User notification of temporary unavailability

### AI Content Generation
**Content Validation Pipeline**:
1. Prompt sanitization and validation
2. AI response appropriateness checking
3. Spotify track ID validation
4. Fallback to text-only questions if tracks unavailable

### Error Handling Strategy
**Resilience Patterns**:
- Circuit breaker for external API failures
- Retry with exponential backoff
- Timeout handling with cancellation tokens
- Graceful degradation with partial functionality

## Performance Targets

### Response Time Goals
- Quiz Generation: <5 seconds (including AI processing)
- Playback Control: <2 seconds
- Device Listing: <1 second
- Health Checks: <500ms

### Scalability Targets
- 100+ concurrent users (constitutional requirement)
- 1000+ quizzes generated per hour
- 10,000+ playback control operations per hour
- 99.9% uptime during business hours

## Development Workflow

### TDD Implementation
**Red-Green-Refactor Cycle**:
1. Write failing test (RED phase)
2. Implement minimal code to pass (GREEN phase)
3. Refactor for quality and performance (REFACTOR phase)
4. Git commit with descriptive message

### Module Development Order
**Parallel Development Capability**:
1. Core entities and interfaces (foundation)
2. Spotify Authentication module [P]
3. Quiz Generation module [P] 
4. Spotify Playback module [P]
5. API Controllers integration
6. End-to-end testing and validation

---
*Research complete. All technical decisions align with constitutional requirements and performance targets.*