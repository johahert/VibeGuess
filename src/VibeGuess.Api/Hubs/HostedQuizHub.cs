using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using VibeGuess.Core.Interfaces;
using VibeGuess.Core.LiveSession;

namespace VibeGuess.Api.Hubs;

/// <summary>
/// SignalR hub for real-time hosted quiz gameplay.
/// Handles host and participant interactions during live quiz sessions.
/// </summary>
public class HostedQuizHub : Hub
{
    private readonly ILiveSessionManager _sessionManager;
    private readonly ILogger<HostedQuizHub> _logger;

    public HostedQuizHub(ILiveSessionManager sessionManager, ILogger<HostedQuizHub> logger)
    {
        _sessionManager = sessionManager;
        _logger = logger;
    }

    // Host Methods
    
    /// <summary>
    /// Create a new live quiz session (Host only)
    /// </summary>
    public async Task<object> CreateSession(string quizId, string title)
    {
        try
        {
            var session = await _sessionManager.CreateSessionAsync(quizId, title, Context.ConnectionId);
            
            // Add host to a group for this session
            await Groups.AddToGroupAsync(Context.ConnectionId, GetSessionGroup(session.SessionId));
            await Groups.AddToGroupAsync(Context.ConnectionId, GetHostGroup(session.SessionId));
            
            _logger.LogInformation("Host {ConnectionId} created session {SessionId} with join code {JoinCode}", 
                Context.ConnectionId, session.SessionId, session.JoinCode);

            // Send host a ready notification with initial session snapshot to signal UI that connection is established
            await Clients.Caller.SendAsync("HostSessionReady", new
            {
                sessionId = session.SessionId,
                joinCode = session.JoinCode,
                participantCount = 0,
                status = session.State.ToString(),
                connectionId = Context.ConnectionId,
                timestamp = DateTime.UtcNow.ToString("O")
            });
            
            return new { success = true, sessionId = session.SessionId, joinCode = session.JoinCode };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create session for host {ConnectionId}", Context.ConnectionId);
            return new { success = false, error = "Failed to create session" };
        }
    }
    
    /// <summary>
    /// Start the quiz game (Host only)
    /// </summary>
    public async Task<object> StartGame(string sessionId)
    {
        try
        {
            var session = await _sessionManager.GetSessionAsync(sessionId);
            if (session == null || session.HostConnectionId != Context.ConnectionId)
            {
                return new { success = false, error = "Unauthorized or session not found" };
            }
            
            var success = await _sessionManager.StartGameAsync(sessionId);
            if (!success)
            {
                return new { success = false, error = "Failed to start game" };
            }
            
            // Notify all participants that the game has started
            await Clients.Group(GetSessionGroup(sessionId)).SendAsync("GameStarted", new { sessionId });
            
            _logger.LogInformation("Game started for session {SessionId} by host {ConnectionId}", sessionId, Context.ConnectionId);
            return new { success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start game for session {SessionId}", sessionId);
            return new { success = false, error = "Failed to start game" };
        }
    }
    
    /// <summary>
    /// Move to next question (Host only)
    /// </summary>
    public async Task<object> NextQuestion(string sessionId, int questionIndex, QuestionData questionData)
    {
        try
        {
            // Validate authorization first
            var session = await _sessionManager.GetSessionAsync(sessionId);
            if (session == null || session.HostConnectionId != Context.ConnectionId)
            {
                _logger.LogWarning("Unauthorized NextQuestion attempt for session {SessionId} from connection {ConnectionId}", 
                    sessionId, Context.ConnectionId);
                return new { success = false, error = "Unauthorized or session not found" };
            }
            
            // Validate question data
            if (questionData == null)
            {
                return new { success = false, error = "Question data is required" };
            }
            
            if (!questionData.IsValid())
            {
                _logger.LogWarning("Invalid question data provided for session {SessionId}: {QuestionText}", 
                    sessionId, questionData.QuestionText);
                return new { success = false, error = "Invalid question data: correct answer must be one of the provided options" };
            }
            
            // Advance to the next question
            var success = await _sessionManager.NextQuestionAsync(sessionId, questionIndex, questionData);
            if (!success)
            {
                return new { success = false, error = "Failed to advance to next question" };
            }
            
            // Get updated session info for broadcasting
            var updatedSession = await _sessionManager.GetSessionAsync(sessionId);
            if (updatedSession == null)
            {
                return new { success = false, error = "Session lost after update" };
            }
            
            // Create participant-safe question data (without correct answer)
            var participantQuestion = questionData.ToParticipantView();
            
            // Broadcast the new question to all participants (without correct answer)
            await Clients.Group(GetSessionGroup(sessionId)).SendAsync("NewQuestion", new 
            { 
                sessionId, 
                questionIndex, 
                question = participantQuestion,
                timeLimit = updatedSession.QuestionTimeLimit,
                startTime = updatedSession.QuestionStartTime?.ToString("O") // ISO 8601 format
            });
            
            // Send question with correct answer to host only (for answer validation)
            await Clients.Group(GetHostGroup(sessionId)).SendAsync("QuestionStarted", new 
            { 
                sessionId, 
                questionIndex, 
                question = questionData, // Full question data including correct answer
                timeLimit = updatedSession.QuestionTimeLimit,
                startTime = updatedSession.QuestionStartTime?.ToString("O"),
                participantCount = updatedSession.Participants.Count
            });
            
            _logger.LogInformation("Advanced to question {QuestionIndex} for session {SessionId}: {QuestionText} (Type: {Type}, Options: {OptionCount})", 
                questionIndex, sessionId, questionData.QuestionText, questionData.Type, questionData.Options.Count);
                
            return new { 
                success = true, 
                questionId = questionData.QuestionId,
                timeLimit = updatedSession.QuestionTimeLimit,
                startTime = updatedSession.QuestionStartTime?.ToString("O")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to advance to next question for session {SessionId}", sessionId);
            return new { success = false, error = "Failed to advance question" };
        }
    }
    
    /// <summary>
    /// End the quiz session (Host only)
    /// </summary>
    public async Task<object> EndSession(string sessionId)
    {
        try
        {
            var session = await _sessionManager.GetSessionAsync(sessionId);
            if (session == null || session.HostConnectionId != Context.ConnectionId)
            {
                return new { success = false, error = "Unauthorized or session not found" };
            }
            
            var success = await _sessionManager.EndGameAsync(sessionId);
            if (!success)
            {
                return new { success = false, error = "Failed to end game" };
            }
            
            // Get final leaderboard
            var leaderboard = await _sessionManager.GetLeaderboardAsync(sessionId);
            
            // Notify all participants that the game has ended
            await Clients.Group(GetSessionGroup(sessionId)).SendAsync("GameEnded", new 
            { 
                sessionId, 
                leaderboard = leaderboard.Take(10).Select(p => new 
                {
                    p.DisplayName,
                    p.Score,
                    p.CorrectAnswers,
                    p.TotalAnswers,
                    Accuracy = p.GetAccuracy()
                })
            });
            
            _logger.LogInformation("Game ended for session {SessionId} by host {ConnectionId}", sessionId, Context.ConnectionId);
            return new { success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to end session {SessionId}", sessionId);
            return new { success = false, error = "Failed to end session" };
        }
    }
    
    /// <summary>
    /// Remove a participant from the session (Host only)
    /// </summary>
    public async Task<object> RemovePlayer(string sessionId, string participantId)
    {
        try
        {
            var session = await _sessionManager.GetSessionAsync(sessionId);
            if (session == null || session.HostConnectionId != Context.ConnectionId)
            {
                return new { success = false, error = "Unauthorized or session not found" };
            }
            
            var participant = await _sessionManager.GetParticipantAsync(sessionId, participantId);
            if (participant == null)
            {
                return new { success = false, error = "Participant not found" };
            }
            
            var success = await _sessionManager.RemoveParticipantAsync(sessionId, participantId);
            if (!success)
            {
                return new { success = false, error = "Failed to remove participant" };
            }
            
            // Notify the removed participant
            await Clients.Client(participant.ConnectionId).SendAsync("RemovedFromSession", new { sessionId, reason = "Removed by host" });
            
            // Notify other participants about the updated participant count
            var updatedParticipants = await _sessionManager.GetParticipantsAsync(sessionId);
            await Clients.Group(GetSessionGroup(sessionId)).SendAsync("ParticipantLeft", new 
            { 
                sessionId, 
                participantId, 
                participantCount = updatedParticipants.Count 
            });
            
            _logger.LogInformation("Participant {ParticipantId} removed from session {SessionId} by host", participantId, sessionId);
            return new { success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove participant {ParticipantId} from session {SessionId}", participantId, sessionId);
            return new { success = false, error = "Failed to remove participant" };
        }
    }
    
    // Player Methods
    
    /// <summary>
    /// Join a live quiz session using join code
    /// </summary>
    public async Task<object> JoinSession(string joinCode, string displayName)
    {
        try
        {
            var session = await _sessionManager.GetSessionByJoinCodeAsync(joinCode);
            if (session == null)
            {
                return new { success = false, error = "Session not found" };
            }
            
            if (session.State != LiveSessionState.Lobby && session.State != LiveSessionState.Active)
            {
                return new { success = false, error = "Session is not accepting new participants" };
            }
            
            // Check if display name is already taken
            var existingParticipants = await _sessionManager.GetParticipantsAsync(session.SessionId);
            if (existingParticipants.Any(p => p.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase)))
            {
                return new { success = false, error = "Display name is already taken" };
            }
            
            var participant = new LiveParticipant
            {
                ParticipantId = Guid.NewGuid().ToString(),
                ConnectionId = Context.ConnectionId,
                DisplayName = displayName,
                JoinedAt = DateTime.UtcNow
            };
            
            var success = await _sessionManager.AddParticipantAsync(session.SessionId, participant);
            if (!success)
            {
                return new { success = false, error = "Failed to join session" };
            }
            
            // Add participant to session group
            await Groups.AddToGroupAsync(Context.ConnectionId, GetSessionGroup(session.SessionId));
            
            // Get updated participant list
            var updatedParticipants = await _sessionManager.GetParticipantsAsync(session.SessionId);
            
            // Notify host about the new participant
            await Clients.Group(GetHostGroup(session.SessionId)).SendAsync("ParticipantJoined", new 
            { 
                sessionId = session.SessionId,
                participant = new { participant.ParticipantId, participant.DisplayName },
                participantCount = updatedParticipants.Count,
                source = "host"
            });
            
            // Notify other participants (exclude new participant and host connection to avoid duplicates on host dashboard)
            var excludedConnections = new List<string> { Context.ConnectionId };
            if (!string.IsNullOrEmpty(session.HostConnectionId))
            {
                excludedConnections.Add(session.HostConnectionId);
            }
            
            await Clients.GroupExcept(GetSessionGroup(session.SessionId), excludedConnections)
                .SendAsync("ParticipantJoined", new 
                { 
                    sessionId = session.SessionId, 
                    participant = new { participant.ParticipantId, participant.DisplayName },
                    participantCount = updatedParticipants.Count,
                    source = "participant"
                });
            
            _logger.LogInformation("Participant {DisplayName} joined session {SessionId}", displayName, session.SessionId);
            return new 
            { 
                success = true, 
                sessionId = session.SessionId, 
                participantId = participant.ParticipantId,
                sessionState = session.State.ToString(),
                currentQuestionIndex = session.CurrentQuestionIndex
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to join session with code {JoinCode}", joinCode);
            return new { success = false, error = "Failed to join session" };
        }
    }
    
    /// <summary>
    /// Submit an answer to the current question
    /// </summary>
    public async Task<object> SubmitAnswer(string sessionId, string participantId, string selectedAnswer)
    {
        try
        {
            var session = await _sessionManager.GetSessionAsync(sessionId);
            if (session == null)
            {
                return new { success = false, error = "Session not found" };
            }
            
            if (session.State != LiveSessionState.Active)
            {
                return new { success = false, error = "Session is not active" };
            }
            
            var participant = await _sessionManager.GetParticipantAsync(sessionId, participantId);
            if (participant == null || participant.ConnectionId != Context.ConnectionId)
            {
                return new { success = false, error = "Unauthorized or participant not found" };
            }
            
            if (participant.HasAnsweredCurrentQuestion)
            {
                return new { success = false, error = "Already answered this question" };
            }
            
            // Validate that there's a current question with valid data
            if (session.CurrentQuestion == null)
            {
                return new { success = false, error = "No current question available" };
            }
            
            // Submit the answer (correct answer validation happens in session manager)
            var answer = await _sessionManager.SubmitAnswerAsync(sessionId, participantId, session.CurrentQuestionIndex, selectedAnswer);
            
            // Notify host about the answer submission
            await Clients.Group(GetHostGroup(sessionId)).SendAsync("AnswerSubmitted", new 
            { 
                sessionId, 
                participantId, 
                participantName = participant.DisplayName,
                questionIndex = session.CurrentQuestionIndex,
                hasAnswered = true
            });
            
            // Update participant scores and get updated leaderboard
            var leaderboard = await _sessionManager.GetLeaderboardAsync(sessionId);
            
            // Broadcast updated leaderboard to all participants
            await Clients.Group(GetSessionGroup(sessionId)).SendAsync("LeaderboardUpdate", new 
            { 
                sessionId, 
                leaderboard = leaderboard.Take(10).Select(p => new 
                {
                    p.ParticipantId,
                    p.DisplayName,
                    p.Score,
                    p.CorrectAnswers,
                    p.TotalAnswers
                })
            });
            
            _logger.LogInformation("Answer submitted for session {SessionId}, participant {ParticipantId}, question {QuestionIndex}: {SelectedAnswer} (Correct: {IsCorrect}, Score: {Score})", 
                sessionId, participantId, session.CurrentQuestionIndex, selectedAnswer, answer.IsCorrect, answer.TotalScore);
            
            return new 
            { 
                success = true, 
                isCorrect = answer.IsCorrect,
                selectedAnswer = answer.SelectedAnswer,
                correctAnswer = answer.IsCorrect ? null : session.CurrentQuestion?.CorrectAnswer, // Only show correct answer if they got it wrong
                score = answer.TotalScore,
                baseScore = answer.BaseScore,
                timeBonus = answer.TimeBonus,
                responseTime = answer.ResponseTime.ToString(@"mm\:ss\.ff"),
                totalScore = participant.Score,
                explanation = session.CurrentQuestion?.Metadata?.Explanation // Show explanation if available
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit answer for session {SessionId}, participant {ParticipantId}", sessionId, participantId);
            return new { success = false, error = "Failed to submit answer" };
        }
    }
    
    /// <summary>
    /// Leave the current session
    /// </summary>
    public async Task<object> LeaveSession(string sessionId, string participantId)
    {
        try
        {
            var participant = await _sessionManager.GetParticipantAsync(sessionId, participantId);
            if (participant != null && participant.ConnectionId == Context.ConnectionId)
            {
                var success = await _sessionManager.RemoveParticipantAsync(sessionId, participantId);
                if (success)
                {
                    // Remove from groups
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetSessionGroup(sessionId));
                    
                    // Notify others about participant leaving
                    var updatedParticipants = await _sessionManager.GetParticipantsAsync(sessionId);
                    await Clients.Group(GetSessionGroup(sessionId)).SendAsync("ParticipantLeft", new 
                    { 
                        sessionId, 
                        participantId, 
                        participantCount = updatedParticipants.Count 
                    });
                    
                    _logger.LogInformation("Participant {ParticipantId} left session {SessionId}", participantId, sessionId);
                }
            }
            
            return new { success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to leave session {SessionId} for participant {ParticipantId}", sessionId, participantId);
            return new { success = false, error = "Failed to leave session" };
        }
    }
    
    // Connection Management
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            // Find sessions where this connection is a participant and mark as disconnected
            // This is a simplified approach - in production, you might want to track connection-to-session mappings
            _logger.LogInformation("Connection {ConnectionId} disconnected", Context.ConnectionId);
            
            // You could implement a cleanup method here to handle disconnected participants
            // For now, we'll rely on the grace period mechanism in the session manager
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disconnect for connection {ConnectionId}", Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
    
    // Helper Methods
    private static string GetSessionGroup(string sessionId) => $"session_{sessionId}";
    private static string GetHostGroup(string sessionId) => $"host_{sessionId}";
}