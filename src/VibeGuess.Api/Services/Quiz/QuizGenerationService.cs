using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using VibeGuess.Api.Services.OpenAI;
using VibeGuess.Api.Services.Spotify;
using VibeGuess.Core.Entities;
using VibeGuess.Infrastructure.Repositories.Interfaces;
using VibeGuess.Spotify.Authentication.Services;

namespace VibeGuess.Api.Services.Quiz;

/// <summary>
/// Service for generating music quizzes using OpenAI and Spotify integration.
/// </summary>
public class QuizGenerationService : IQuizGenerationService
{
    private readonly OpenAISettings _openAiSettings;
    private readonly ISpotifyAuthenticationService _spotifyAuth;
    private readonly ISpotifyApiService _spotifyApiService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly HttpClient _httpClient;
    private readonly ILogger<QuizGenerationService> _logger;

    public QuizGenerationService(
        IOptions<OpenAISettings> openAiSettings,
        ISpotifyAuthenticationService spotifyAuth,
        ISpotifyApiService spotifyApiService,
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory,
        ILogger<QuizGenerationService> logger)
    {
        _openAiSettings = openAiSettings.Value;
        _spotifyAuth = spotifyAuth;
        _spotifyApiService = spotifyApiService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiSettings.ApiKey}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "VibeGuess/1.0");
    }

    public async Task<QuizGenerationResult> GenerateQuizAsync(QuizGenerationRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var metadata = new QuizGenerationMetadata
        {
            AiModel = _openAiSettings.Model,
            GenerationSteps = new List<string>()
        };

        try
        {
            _logger.LogInformation("Starting quiz generation for user {UserId} with prompt: {Prompt}", userId, request.Prompt);
            metadata.GenerationSteps.Add("Started quiz generation process");

            // Step 1: Generate quiz concept and questions using OpenAI
            metadata.GenerationSteps.Add("Generating questions with OpenAI");
            var questionsData = await GenerateQuestionsWithOpenAI(request, cancellationToken);
            
            if (questionsData == null)
            {
                return new QuizGenerationResult
                {
                    Success = false,
                    ErrorMessage = "Failed to generate questions with OpenAI",
                    Metadata = metadata
                };
            }

            // Step 2: Create the quiz entity
            metadata.GenerationSteps.Add("Creating quiz entity");
            var quiz = new VibeGuess.Core.Entities.Quiz
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = questionsData.Title,
                UserPrompt = request.Prompt,
                Format = request.Format,
                Difficulty = request.Difficulty,
                QuestionCount = request.QuestionCount,
                Language = request.Language,
                IncludesAudio = request.IncludeAudio,
                Status = "Generated",
                IsPublic = false,
                CreatedAt = DateTime.UtcNow
            };

            quiz.UpdateTimestamp(); // Sets expiration to 30 days from now

            // Step 3: Process each question and find Spotify tracks
            metadata.GenerationSteps.Add("Processing questions and finding Spotify tracks");
            var questions = new List<Question>();
            
            for (int i = 0; i < questionsData.Questions.Count && i < request.QuestionCount; i++)
            {
                var questionData = questionsData.Questions[i];
                
                var question = new Question
                {
                    Id = Guid.NewGuid(),
                    QuizId = quiz.Id,
                    OrderIndex = i,
                    QuestionText = questionData.QuestionText,
                    Type = request.Format,
                    RequiresAudio = request.IncludeAudio,
                    Points = 10,
                    HintText = questionData.Hint,
                    Explanation = questionData.Explanation
                };

                // Find Spotify track for this question
                if (!string.IsNullOrEmpty(questionData.TrackInfo?.TrackName) && 
                    !string.IsNullOrEmpty(questionData.TrackInfo?.ArtistName))
                {
                    var track = await FindSpotifyTrack(questionData.TrackInfo.TrackName, questionData.TrackInfo.ArtistName);
                    if (track != null)
                    {
                        question.Track = track;
                        metadata.TracksFound++;
                    }
                    else
                    {
                        metadata.TracksFailed++;
                        metadata.Warnings.Add($"Could not find Spotify track: {questionData.TrackInfo.ArtistName} - {questionData.TrackInfo.TrackName}");
                    }
                }

                // Add answer options
                if (questionData.AnswerOptions != null)
                {
                    for (int j = 0; j < questionData.AnswerOptions.Count; j++)
                    {
                        var optionData = questionData.AnswerOptions[j];
                        var answerOption = new AnswerOption
                        {
                            Id = Guid.NewGuid(),
                            QuestionId = question.Id,
                            OrderIndex = j,
                            AnswerText = optionData.Text,
                            IsCorrect = optionData.IsCorrect
                        };
                        question.AnswerOptions.Add(answerOption);
                    }
                }

                questions.Add(question);
            }

            quiz.Questions = questions;
            metadata.TracksValidated = metadata.TracksFound;

            stopwatch.Stop();
            metadata.ProcessingTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            metadata.GenerationSteps.Add($"Quiz generation completed in {metadata.ProcessingTimeMs:F0}ms");

            _logger.LogInformation("Quiz generation completed successfully. Quiz ID: {QuizId}, Questions: {QuestionCount}, Tracks: {TracksFound}/{TracksTotal}", 
                quiz.Id, questions.Count, metadata.TracksFound, questions.Count);

            return new QuizGenerationResult
            {
                Success = true,
                Quiz = quiz,
                Metadata = metadata
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            metadata.ProcessingTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            
            _logger.LogError(ex, "Error generating quiz for user {UserId}: {Error}", userId, ex.Message);
            
            return new QuizGenerationResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Metadata = metadata
            };
        }
    }

    private async Task<GeneratedQuizData?> GenerateQuestionsWithOpenAI(QuizGenerationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var systemPrompt = CreateSystemPrompt(request);
            var userPrompt = CreateUserPrompt(request);

            var requestBody = new
            {
                model = _openAiSettings.Model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                max_completion_tokens = 4000,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API request failed: {StatusCode} - {Response}", response.StatusCode, responseContent);
                return null;
            }

            // Parse OpenAI response
            using var doc = JsonDocument.Parse(responseContent);
            var choices = doc.RootElement.GetProperty("choices");
            if (choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                var aiResponse = message.GetProperty("content").GetString();

                return ParseOpenAIResponse(aiResponse);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API for quiz generation");
            return null;
        }
    }

    private string CreateSystemPrompt(QuizGenerationRequest request)
    {
        return $@"You are a music quiz generator. Create engaging music quiz questions based on user prompts.

Rules:
1. Generate exactly {request.QuestionCount} questions
2. Format: {request.Format}
3. Difficulty: {request.Difficulty}
4. Include audio tracks when possible: {request.IncludeAudio}
5. Each question should have 4 multiple choice options with exactly one correct answer
6. Include hints and brief explanations
7. Focus on real songs, artists, albums, and music history
8. Provide specific track and artist names for suited for Spotify lookup

Response format (JSON):
{{
  ""title"": ""Quiz Title"",
  ""description"": ""Brief description"",
  ""questions"": [
    {{
      ""questionText"": ""Question text"",
      ""hint"": ""Optional hint"",
      ""explanation"": ""Brief explanation of the answer"",
      ""trackInfo"": {{
        ""trackName"": ""Song Title"",
        ""artistName"": ""Artist Name"",
        ""albumName"": ""Album Name""
      }},
      ""answerOptions"": [
        {{""text"": ""Option 1"", ""isCorrect"": true}},
        {{""text"": ""Option 2"", ""isCorrect"": false}},
        {{""text"": ""Option 3"", ""isCorrect"": false}},
        {{""text"": ""Option 4"", ""isCorrect"": false}}
      ]
    }}
  ]
}}";
    }

    private string CreateUserPrompt(QuizGenerationRequest request)
    {
        var difficultyGuidance = request.Difficulty.ToLower() switch
        {
            "easy" => "Focus on very well-known songs and mainstream artists that most people would recognize.",
            "medium" => "Include a mix of popular and moderately obscure songs. Some deep cuts are okay.",
            "hard" => "Include challenging questions about B-sides, album tracks, music theory, and lesser-known facts.",
            _ => "Use moderate difficulty with popular songs and artists."
        };

        return $@"Create a music quiz based on this prompt: ""{request.Prompt}""

Difficulty guidance: {difficultyGuidance}

Make sure to:
- Use real, verifiable songs and artists
- Provide accurate track and artist names for Spotify lookup
- Make questions engaging and educational
- Include variety in question types (song identification, artist facts, album info, etc.)
- Ensure all answer options are plausible

Respond only with valid JSON in the specified format.";
    }

    private GeneratedQuizData? ParseOpenAIResponse(string? aiResponse)
    {
        if (string.IsNullOrEmpty(aiResponse))
            return null;

        try
        {
            // Clean up the response - remove markdown code blocks if present
            var cleanedResponse = aiResponse.Trim();
            if (cleanedResponse.StartsWith("```json"))
            {
                cleanedResponse = cleanedResponse.Substring(7);
            }
            if (cleanedResponse.StartsWith("```"))
            {
                cleanedResponse = cleanedResponse.Substring(3);
            }
            if (cleanedResponse.EndsWith("```"))
            {
                cleanedResponse = cleanedResponse.Substring(0, cleanedResponse.Length - 3);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<GeneratedQuizData>(cleanedResponse, options);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI response as JSON: {Response}", aiResponse);
            return null;
        }
    }

    private async Task<Track?> FindSpotifyTrack(string trackName, string artistName)
    {
        try
        {
            _logger.LogInformation("Looking for track: {Artist} - {Track}", artistName, trackName);

            // Step 1: Check if track already exists in database
            var existingTracks = await _unitOfWork.Tracks.SearchAsync($"{artistName} {trackName}");
            var exactMatch = existingTracks.FirstOrDefault(t => 
                string.Equals(t.Name, trackName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(t.ArtistName, artistName, StringComparison.OrdinalIgnoreCase));

            if (exactMatch != null)
            {
                _logger.LogInformation("Found existing track in database: {TrackId} - {Artist} - {Track}", 
                    exactMatch.Id, exactMatch.ArtistName, exactMatch.Name);
                return exactMatch;
            }

            // Step 2: Search Spotify API for the track
            var spotifyTrack = await _spotifyApiService.SearchTrackAsync(trackName, artistName);
            
            if (spotifyTrack != null)
            {
                // Step 3: Check if this Spotify track already exists in database
                var existingBySpotifyId = await _unitOfWork.Tracks.GetBySpotifyTrackIdAsync(spotifyTrack.SpotifyTrackId);
                if (existingBySpotifyId != null)
                {
                    _logger.LogInformation("Found existing Spotify track in database: {SpotifyId}", spotifyTrack.SpotifyTrackId);
                    return existingBySpotifyId;
                }

                // Step 4: Save new track to database
                var savedTrack = await _unitOfWork.Tracks.AddAsync(spotifyTrack);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Saved new Spotify track: {SpotifyId} - {Artist} - {Track}", 
                    savedTrack.SpotifyTrackId, savedTrack.ArtistName, savedTrack.Name);
                
                return savedTrack;
            }

            // Step 5: If Spotify API fails, create a mock track for fallback
            _logger.LogWarning("Spotify API search failed for {Artist} - {Track}, creating mock track", artistName, trackName);
            
            var mockTrack = new Track
            {
                Id = Guid.NewGuid(),
                SpotifyTrackId = $"mock:track:{Guid.NewGuid():N}",
                Name = trackName,
                ArtistName = artistName,
                AlbumName = "Unknown Album",
                DurationMs = 180000, // Default 3 minutes
                Popularity = 50, // Medium popularity
                IsExplicit = false,
                PreviewUrl = null,
                CreatedAt = DateTime.UtcNow
            };

            // Save mock track to avoid repeated API calls for the same failed search
            var savedMockTrack = await _unitOfWork.Tracks.AddAsync(mockTrack);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created and saved mock track: {TrackId} - {Artist} - {Track}", 
                savedMockTrack.Id, artistName, trackName);
            
            return savedMockTrack;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding/creating track for {Artist} - {Track}", artistName, trackName);
            return null;
        }
    }
}

// Helper classes for parsing OpenAI responses
public class GeneratedQuizData
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<GeneratedQuestion> Questions { get; set; } = new();
}

public class GeneratedQuestion
{
    public string QuestionText { get; set; } = string.Empty;
    public string? Hint { get; set; }
    public string? Explanation { get; set; }
    public GeneratedTrackInfo? TrackInfo { get; set; }
    public List<GeneratedAnswerOption> AnswerOptions { get; set; } = new();
}

public class GeneratedTrackInfo
{
    public string TrackName { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public string? AlbumName { get; set; }
}

public class GeneratedAnswerOption
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}