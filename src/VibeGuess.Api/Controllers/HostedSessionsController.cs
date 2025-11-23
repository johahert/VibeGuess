using Microsoft.AspNetCore.Mvc;
using VibeGuess.Api.Models.Requests;
using VibeGuess.Api.Models.Responses;
using VibeGuess.Core.Interfaces;
using VibeGuess.Core.LiveSession;

namespace VibeGuess.Api.Controllers;

/// <summary>
/// REST API controller for hosted quiz session lifecycle management.
/// Provides minimal endpoints for session creation, info retrieval, and analytics.
/// Primary interaction happens via SignalR hub - these are supplementary endpoints.
/// </summary>
[Route("api/[controller]")]
public class HostedSessionsController : BaseApiController
{
    private readonly ILiveSessionManager _sessionManager;
    private readonly ILogger<HostedSessionsController> _logger;

    public HostedSessionsController(ILiveSessionManager sessionManager, ILogger<HostedSessionsController> logger)
    {
        _sessionManager = sessionManager;
        _logger = logger;
    }

    /// <summary>
    /// Create a new hosted quiz session
    /// </summary>
    /// <param name="request">Session creation parameters</param>
    /// <returns>Created session information including join code</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateHostedSessionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateSession([FromBody] CreateHostedSessionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequestWithError("Invalid request parameters");
            }

            // Generate a temporary connection ID for the REST endpoint
            // In practice, the host would connect via SignalR first
            var tempConnectionId = $"rest_{Guid.NewGuid()}";
            
            var session = await _sessionManager.CreateSessionAsync(request.QuizId, request.Title, tempConnectionId);
            
            // Update the question time limit if specified
            if (request.QuestionTimeLimit != 30)
            {
                session.QuestionTimeLimit = request.QuestionTimeLimit;
                await _sessionManager.UpdateSessionAsync(session);
            }

            var response = new CreateHostedSessionResponse
            {
                SessionId = session.SessionId,
                JoinCode = session.JoinCode,
                Title = session.Title,
                State = session.State.ToString(),
                CreatedAt = session.CreatedAt
            };

            _logger.LogInformation("Hosted session created via REST API: {SessionId} with join code {JoinCode}", 
                session.SessionId, session.JoinCode);

            return OkWithHeaders(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create hosted session via REST API");
            return CreateErrorResponse(500, "internal_error", "Failed to create session");
        }
    }

    /// <summary>
    /// Get information about a hosted session by join code
    /// </summary>
    /// <param name="joinCode">Session join code</param>
    /// <returns>Session information and current state</returns>
    [HttpGet("{joinCode}")]
    [ProducesResponseType(typeof(HostedSessionInfoResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetSessionInfo(string joinCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(joinCode))
            {
                return BadRequestWithError("Join code is required");
            }

            var session = await _sessionManager.GetSessionByJoinCodeAsync(joinCode);
            if (session == null)
            {
                return NotFound();
            }

            var participants = await _sessionManager.GetParticipantsAsync(session.SessionId);
            
            var response = new HostedSessionInfoResponse
            {
                SessionId = session.SessionId,
                JoinCode = session.JoinCode,
                Title = session.Title,
                State = session.State.ToString(),
                CurrentQuestionIndex = session.CurrentQuestionIndex,
                ParticipantCount = participants.Count,
                CreatedAt = session.CreatedAt,
                StartedAt = session.StartedAt,
                EndedAt = session.EndedAt,
                QuestionTimeLimit = session.QuestionTimeLimit,
                Participants = participants.Select(p => new ParticipantSummary
                {
                    ParticipantId = p.ParticipantId,
                    DisplayName = p.DisplayName,
                    Score = p.Score,
                    CorrectAnswers = p.CorrectAnswers,
                    TotalAnswers = p.TotalAnswers,
                    IsConnected = p.IsConnected,
                    JoinedAt = p.JoinedAt
                }).ToList()
            };

            return OkWithHeaders(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve session info for join code {JoinCode}", joinCode);
            return CreateErrorResponse(500, "internal_error", "Failed to retrieve session information");
        }
    }

    /// <summary>
    /// Get analytics summary for a completed session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Comprehensive analytics and statistics for the session</returns>
    [HttpGet("{sessionId}/summary")]
    [ProducesResponseType(typeof(SessionSummaryResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetSessionSummary(string sessionId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return BadRequestWithError("Session ID is required");
            }

            var session = await _sessionManager.GetSessionAsync(sessionId);
            if (session == null)
            {
                return NotFound();
            }

            // Note: This endpoint is primarily for completed sessions but can work with active ones too
            var participants = await _sessionManager.GetParticipantsAsync(sessionId);
            var leaderboard = await _sessionManager.GetLeaderboardAsync(sessionId);

            // Calculate session statistics
            var stats = new SessionStats
            {
                TotalParticipants = participants.Count,
                TotalQuestions = session.CurrentQuestionIndex + (session.State == LiveSessionState.Completed ? 1 : 0),
                TotalAnswers = participants.Sum(p => p.TotalAnswers),
                AverageScore = participants.Count > 0 ? participants.Average(p => p.Score) : 0,
                AverageAccuracy = participants.Count > 0 ? participants.Average(p => p.GetAccuracy()) : 0
            };

            // Calculate question-level statistics
            var questionStats = new List<QuestionStats>();
            if (session.Answers.Count > 0)
            {
                var questionGroups = session.Answers.GroupBy(a => a.QuestionIndex);
                foreach (var group in questionGroups.OrderBy(g => g.Key))
                {
                    var answers = group.ToList();
                    questionStats.Add(new QuestionStats
                    {
                        QuestionIndex = group.Key,
                        TotalAnswers = answers.Count,
                        CorrectAnswers = answers.Count(a => a.IsCorrect),
                        AverageResponseTime = answers.Count > 0 
                            ? TimeSpan.FromTicks((long)answers.Average(a => a.ResponseTime.Ticks))
                            : TimeSpan.Zero
                    });
                }

                // Update average response time in stats
                if (session.Answers.Count > 0)
                {
                    stats.AverageResponseTime = TimeSpan.FromTicks((long)session.Answers.Average(a => a.ResponseTime.Ticks));
                }
            }

            var response = new SessionSummaryResponse
            {
                SessionId = session.SessionId,
                Title = session.Title,
                CreatedAt = session.CreatedAt,
                StartedAt = session.StartedAt,
                EndedAt = session.EndedAt,
                Stats = stats,
                FinalLeaderboard = leaderboard.Take(20).Select(p => new ParticipantSummary
                {
                    ParticipantId = p.ParticipantId,
                    DisplayName = p.DisplayName,
                    Score = p.Score,
                    CorrectAnswers = p.CorrectAnswers,
                    TotalAnswers = p.TotalAnswers,
                    IsConnected = p.IsConnected,
                    JoinedAt = p.JoinedAt
                }).ToList(),
                QuestionStats = questionStats
            };

            return OkWithHeaders(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve session summary for {SessionId}", sessionId);
            return CreateErrorResponse(500, "internal_error", "Failed to retrieve session summary");
        }
    }
}