using System.ComponentModel.DataAnnotations;

namespace VibeGuess.Api.Models.Requests;

/// <summary>
/// Request model for creating a new hosted quiz session
/// </summary>
public class CreateHostedSessionRequest
{
    [Required]
    [MaxLength(100)]
    public string QuizId { get; set; } = string.Empty;
    
    [Required] 
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Range(10, 300)]
    public int QuestionTimeLimit { get; set; } = 30; // seconds
}