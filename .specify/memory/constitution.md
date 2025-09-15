# VibeGuess Constitution
<!-- AI-powered Spotify Quiz Generator Web API -->

## Core Principles

### I. Security-First Architecture
All authentication and authorization must be implemented using industry standards; OAuth 2.0 with PKCE for Spotify integration; JWT tokens with proper expiration and refresh mechanisms; All API endpoints require authentication except health checks; Sensitive data (tokens, user info) encrypted at rest and in transit; Rate limiting implemented for all external API calls.

### II. API-First Design
RESTful API design following OpenAPI 3.0 specification; All endpoints documented with Swagger/OpenAPI; Consistent response formats with proper HTTP status codes; Input validation on all endpoints; Standardized error responses with correlation IDs; Content negotiation support (JSON primary, with versioning headers).

### III. Test-Driven Development (NON-NEGOTIABLE)
Unit tests written before implementation for all business logic; Integration tests required for Spotify API integration; End-to-end tests for complete quiz generation workflows; Mocked external dependencies in unit tests; Test coverage minimum 80% for core business logic; All tests must pass before merge to main branch.

### IV. External API Integration Standards
Resilient integration patterns: Circuit breaker, retry with exponential backoff, timeout handling; Spotify API rate limiting compliance (respect 429 responses); Proper error handling and logging for API failures; Offline/cached mode when possible; Health checks for external dependencies; Configuration-driven API endpoints (dev/staging/prod).

### V. AI Integration & Data Quality
AI prompts must be validated and sanitized before processing; Generated content reviewed for appropriateness and accuracy; Fallback mechanisms when AI services are unavailable; Audit trail for all AI-generated content; Performance monitoring for AI response times; Cost monitoring and budget controls for AI API usage.

## Technical Requirements

### Technology Stack
- **.NET 8.0** or later with ASP.NET Core Web API
- **Entity Framework Core** for data persistence
- **SQL Server** or **PostgreSQL** for primary database
- **Redis** for caching and session management
- **Serilog** for structured logging
- **AutoMapper** for object mapping
- **FluentValidation** for input validation
- **Polly** for resilience patterns
- **MediatR** for CQR pattern implementation

### Spotify Integration Requirements
- **Spotify Web API** integration using OAuth 2.0 Authorization Code Flow with PKCE
- Support for user authentication and authorization scopes: `user-read-private`, `user-top-read`, `playlist-read-private`
- Ability to fetch user's top tracks, artists, and playlists
- Respect Spotify API rate limits (100 requests per minute per application)
- Implement token refresh mechanism for long-lived sessions
- Handle Spotify API errors gracefully with appropriate user feedback

### AI Integration Requirements
- Integration with **OpenAI GPT-4** or **Azure OpenAI Service** for quiz generation
- Support for customizable AI prompts based on music data
- Generated quiz types: Multiple choice, True/False, Fill-in-the-blank
- Content moderation filters for generated questions
- Performance target: Quiz generation within 10 seconds
- Cost monitoring with configurable usage limits

### Performance & Scalability
- API response times under 2 seconds for quiz generation
- Support for concurrent users (minimum 100 simultaneous)
- Horizontal scaling capability with stateless design
- Database connection pooling and query optimization
- Caching strategy for frequently accessed data (user profiles, generated quizzes)

## Development Workflow & Quality Gates

### Code Standards
- **C# 12** language features and nullable reference types enabled
- **Clean Architecture** pattern with clear separation of concerns
- **SOLID principles** strictly enforced
- **Async/await** patterns for all I/O operations
- **Configuration-driven** behavior (appsettings.json, environment variables)
- **Dependency injection** container for all services
- **Repository pattern** for data access abstraction

### Security Requirements
- **HTTPS only** in production with TLS 1.2+
- **CORS** properly configured for known origins
- **Input sanitization** and validation on all endpoints
- **SQL injection** prevention through parameterized queries
- **Secrets management** using Azure Key Vault or similar
- **Security headers** implemented (HSTS, CSP, X-Frame-Options)
- **Audit logging** for all user actions and data access

### Deployment & Environment Management
- **Containerized deployment** using Docker
- **Environment-specific configurations** (Development, Staging, Production)
- **Health checks** for application, database, and external dependencies
- **Monitoring and alerting** using Application Insights or similar
- **Blue-green deployment** strategy for zero-downtime updates
- **Database migrations** using EF Core migrations with rollback capability
- **Feature flags** for gradual feature rollout and A/B testing

## Governance

### Code Review Requirements
- All pull requests require at least one approval from a senior developer
- Security-sensitive changes require approval from security reviewer
- External API integration changes require architecture review
- Database schema changes require DBA review and migration testing
- Performance-impacting changes require benchmarking results

### Quality Gates
- All unit tests must pass (minimum 80% coverage)
- Integration tests must pass for Spotify and AI integrations
- Static code analysis (SonarQube) must show no critical issues
- Security scanning (OWASP ZAP or similar) must pass
- Performance tests must meet established benchmarks
- Documentation must be updated for API changes

### Compliance & Monitoring
- GDPR compliance for user data handling and retention
- Spotify Developer Terms of Service adherence
- AI service terms compliance and content moderation
- Regular security audits and penetration testing
- Cost monitoring and budget alerts for cloud services
- User analytics and privacy-compliant usage tracking

**Version**: 1.0.0 | **Ratified**: 2025-09-15 | **Last Amended**: 2025-09-15