# VibeGuess 🎵


> **AI-Powered Music Quiz Game with Spotify Integration**

VibeGuess is an interactive music quiz application that combines artificial intelligence, Spotify's rich music catalog, and real-time audio playback to create engaging musical trivia experiences. Users can generate custom quizzes on any music topic and play along with authentic track previews.

## 🎯 **Project Vision**

Transform how people discover and engage with music through personalized, AI-generated quizzes that test knowledge while introducing users to new songs and artists via Spotify's extensive library.

## ✨ **Key Features**

### 🤖 **AI-Powered Quiz Generation**
- **Natural Language Prompts**: Generate quizzes from simple descriptions like "80s rock bands and their hit songs"
- **Smart Content Creation**: GPT-4 powered question generation with contextual knowledge
- **Flexible Difficulty**: Easy, Medium, and Hard difficulty levels with adaptive scoring
- **Multiple Formats**: Multiple choice, free text, and mixed question types

### 🎵 **Spotify Integration**
- **OAuth 2.0 PKCE Authentication**: Secure Spotify account connection
- **Real-Time Playback Control**: Play track previews directly in the browser
- **Multi-Device Support**: Control playback across Spotify Connect devices
- **Track Validation**: Ensures quiz tracks are playable in user's region

### 📊 **Interactive Quiz Experience**
- **Live Audio Playback**: Hear actual track snippets during questions
- **Progress Tracking**: Real-time scoring and session management
- **Quiz History**: Save and replay favorite quizzes
- **Social Features**: Share quiz results and challenge friends

### 🏗️ **Modern Architecture**
- **.NET 8 Web API**: High-performance backend with modular design
- **Entity Framework Core**: Robust data persistence with SQL Server
- **JWT Authentication**: Secure token-based user sessions
- **Rate Limiting**: Prevent abuse and ensure fair usage
- **Comprehensive Testing**: TDD approach with contract and integration tests

## 🛠️ **Technology Stack**

### **Backend (.NET 8)**
- **ASP.NET Core Web API**: RESTful API endpoints
- **Entity Framework Core**: Database ORM with migrations
- **AutoMapper**: Object mapping for DTOs
- **FluentValidation**: Input validation and sanitization
- **Polly**: Resilience patterns for external API calls
- **Serilog**: Structured logging and observability

### **External Integrations**
- **Spotify Web API**: Music catalog and playback control
- **OpenAI GPT-4**: AI-powered content generation
- **SQL Server**: Primary database with Testcontainers for testing

### **Testing & Quality**
- **xUnit**: Unit and integration testing framework
- **Moq**: Mocking framework for isolated testing
- **Microsoft.AspNetCore.Mvc.Testing**: API integration testing
- **Testcontainers**: Database testing with real SQL Server instances

## 📁 **Project Structure**

```
VibeGuess/
├── src/
│   ├── VibeGuess.Api/                    # Main Web API project
│   ├── VibeGuess.Core/                   # Domain entities and interfaces
│   ├── VibeGuess.Infrastructure/         # Data access and EF Core
│   ├── VibeGuess.Spotify.Authentication/ # OAuth 2.0 PKCE implementation
│   ├── VibeGuess.Quiz.Generation/        # AI quiz generation service
│   └── VibeGuess.Spotify.Playback/       # Spotify Web API client
├── tests/
│   ├── VibeGuess.Api.Tests/              # API contract and integration tests
│   ├── VibeGuess.Integration.Tests/      # End-to-end workflow tests
│   ├── VibeGuess.Infrastructure.Tests/   # Database integration tests
│   ├── VibeGuess.Quiz.Tests/             # Quiz generation unit tests
│   └── VibeGuess.Spotify.Tests/          # Spotify integration tests
├── specs/                                # Design documents and contracts
└── docs/                                 # Additional documentation
```

## 🚀 **Current Development Status**

### ✅ **Completed (Phase 1)**
- **Project Setup**: Complete .NET 8 solution with modular architecture
- **Test Infrastructure**: xUnit, Moq, Testcontainers configuration
- **Contract Tests**: Authentication and Quiz API contract validation (34 tests)
- **TDD Foundation**: RED phase complete - all tests properly failing

### 🔄 **In Progress (Phase 2)**
- **Remaining Contract Tests**: Playback and Health API endpoints
- **Integration Test Suite**: End-to-end workflow scenarios
- **Entity Model Creation**: Domain models per data specifications

### ⏳ **Planned (Phase 3)**
- **Service Implementation**: Business logic and external API clients
- **Database Layer**: EF Core context, repositories, and migrations  
- **API Controllers**: REST endpoints with authentication middleware
- **Performance Optimization**: Caching, async patterns, and monitoring

## 🎮 **API Overview**

### **Authentication Endpoints**
```http
POST /api/auth/spotify/login      # Initiate Spotify OAuth flow
POST /api/auth/spotify/callback   # Complete OAuth and get tokens
POST /api/auth/refresh            # Refresh expired access tokens
GET  /api/auth/me                 # Get user profile and settings
```

### **Quiz Management Endpoints**
```http
POST /api/quiz/generate           # Generate AI-powered quiz
GET  /api/quiz/{id}              # Retrieve specific quiz
GET  /api/quiz/my-quizzes        # Get user's quiz history
POST /api/quiz/{id}/start-session # Start interactive quiz session
```

### **Playback Control Endpoints**
```http
GET  /api/playback/devices        # List available Spotify devices
POST /api/playback/play           # Start track playback
POST /api/playback/pause          # Pause current playback
GET  /api/playback/status         # Get current playback state
```

### **Health & Monitoring Endpoints**
```http
GET  /api/health                  # API health status
POST /api/health/test/spotify     # Test Spotify connectivity
POST /api/health/test/openai      # Test AI service availability
```

## 📊 **Development Progress**

- **Overall Progress**: 20% (13/65 tasks completed)
- **Contract Tests**: 36% (5/14 endpoints tested)
- **Test Coverage**: 34+ contract tests ensuring API compliance
- **Architecture**: Modular design with clean separation of concerns

## 🛡️ **Security & Compliance**

- **OAuth 2.0 PKCE**: Secure authentication without client secrets
- **JWT Tokens**: Stateless authentication with 1-hour expiration
- **Rate Limiting**: API protection against abuse (configurable limits)
- **Input Validation**: Comprehensive request sanitization
- **CORS Policy**: Secure cross-origin resource sharing
- **HTTPS Enforcement**: TLS encryption for all communications

## 🎵 **Sample Quiz Generation**

```json
{
  "prompt": "Create a quiz about 80s rock bands and their hit songs",
  "questionCount": 10,
  "format": "MultipleChoice",
  "difficulty": "Medium",
  "includeAudio": true
}
```

**Generated Quiz Preview:**
- **Question**: "Which band released 'Don't Stop Believin'' in 1981?"
- **Audio**: 30-second Spotify preview of the actual track
- **Options**: Journey, Foreigner, REO Speedwagon, Boston
- **Metadata**: Track info, album art, and playback controls

## 🚧 **Getting Started**

### **Prerequisites**
- .NET 8 SDK
- SQL Server (or Docker for Testcontainers)
- Spotify Developer Account
- OpenAI API Key (for quiz generation)

### **Development Setup**
```bash
# Clone repository
git clone https://github.com/johahert/VibeGuess.git
cd VibeGuess

# Restore packages
dotnet restore src/VibeGuess.sln

# Run contract tests (should fail - TDD RED phase)
dotnet test --filter "ContractTests"

# Build solution
dotnet build src/VibeGuess.sln
```

### **Configuration**
```json
{
  "Spotify": {
    "ClientId": "your-spotify-client-id",
    "RedirectUri": "https://localhost:5001/callback"
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "Model": "gpt-4"
  }
}
```

## 📈 **Roadmap**

### **V1.0 - Core Features** (Current)
- Basic quiz generation and playback
- Spotify authentication and device control
- Simple web interface

### **V2.0 - Social Features** (Future)
- Multi-player quiz rooms
- Leaderboards and achievements
- Quiz sharing and community features

### **V3.0 - Advanced AI** (Future)
- Personalized quiz recommendations
- Adaptive difficulty based on performance
- Advanced music analysis and insights

## 🤝 **Contributing**

This project follows Test-Driven Development (TDD) principles:
1. **RED**: Write failing tests first
2. **GREEN**: Implement minimal code to pass tests
3. **REFACTOR**: Improve code while maintaining tests

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

## 📄 **License**

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.

## 🎶 **Powered By**

- [Spotify Web API](https://developer.spotify.com/documentation/web-api/) - Music catalog and playback
- [OpenAI GPT-4](https://openai.com/gpt-4) - AI-powered content generation
- [.NET 8](https://dotnet.microsoft.com/) - High-performance web API framework

---

**🎵 Ready to test your music knowledge? Let VibeGuess create the perfect quiz for you!**