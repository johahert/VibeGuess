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
                    var track = await FindSpotifyTrackWithRetry(questionData.TrackInfo.TrackName, questionData.TrackInfo.ArtistName);
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
            // Generate buffer questions (50% more) to account for quality filtering and track validation failures
            var bufferRequest = new QuizGenerationRequest
            {
                Prompt = request.Prompt,
                QuestionCount = Math.Max(request.QuestionCount + 5, (int)(request.QuestionCount * 1.5)), // At least 5 extra, or 50% more
                Difficulty = request.Difficulty,
                Format = request.Format,
                Language = request.Language,
                IncludeAudio = request.IncludeAudio
            };

            var systemPrompt = CreateSystemPrompt(bufferRequest);
            var userPrompt = CreateUserPrompt(bufferRequest);

            var requestBody = new
            {
                model = _openAiSettings.Model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                max_completion_tokens = 5000, // Increased for more questions
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

                var quizData = ParseOpenAIResponse(aiResponse);
                
                // Log the buffer generation results
                if (quizData?.Questions != null)
                {
                    _logger.LogInformation("Generated {GeneratedCount} questions (requested {RequestedCount} plus buffer) for target of {TargetCount}", 
                        quizData.Questions.Count, bufferRequest.QuestionCount, request.QuestionCount);
                }

                return quizData;
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

CRITICAL: All songs MUST be real, well-known tracks that exist on Spotify. Use mainstream artists and their popular songs.

Rules:
1. Generate exactly {request.QuestionCount} questions
2. Format: {request.Format}
3. Difficulty: {request.Difficulty}
4. Include audio tracks when possible: {request.IncludeAudio}
5. Each question should have 4 multiple choice options with exactly one correct answer
6. Include hints and brief explanations
7. Focus on REAL songs, artists, albums, and music history - NO fictional content
8. Use EXACT track and artist names as they appear on Spotify (no nicknames or variations)
9. Prioritize well-known songs over deep cuts to ensure Spotify availability
10. Use standard English characters only (avoid special characters, emojis, or non-ASCII text)

QUESTION VARIETY REQUIREMENTS:
NEVER include the song title or obvious lyrics in the question text. Create diverse question types:

- Release year questions: ""In what year was this song by [Artist] released?"" 
- Album questions: ""Which album features this song by [Artist]?""
- Artist identification: ""Who performed this hit song from [year/album]?""
- Chart performance: ""This [Artist] song reached #[X] on the Billboard Hot 100 in [year]. What song is it?""
- Collaboration questions: ""Which artist collaborated with [Artist] on this [year] hit?""
- Genre/style questions: ""This [genre] song by [Artist] became a hit in [year]. What is it?""
- Award questions: ""Which [Artist] song won [award] in [year]?""
- Cover/original questions: ""This song was originally performed by [Artist] in [year]. What is the title?""
- Soundtrack questions: ""Which [Artist] song was featured in the movie [Film]?""
- Band member questions: ""Which song marked [member]'s debut with [Band]?""

FORBIDDEN QUESTION PATTERNS:
❌ ""What song contains the lyrics '[exact lyrics]'?"" 
❌ ""Complete this lyric: '[song title lyrics]'""
❌ ""What song starts with '[first line that contains title]'?""
❌ Any question where the song title appears in the question text

TRACK SELECTION GUIDELINES:
- Use chart hits, popular songs, and widely-known tracks
- Prefer main releases over remixes or special editions  
- Use primary artist names (avoid ""feat."" or multiple artists when possible)
- Use official song titles without subtitles in parentheses
- Double-check that artist and song combinations are real and correct

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
            "easy" => "Focus on chart-topping hits and mainstream artists from the past 20 years that are definitely on Spotify. Use basic facts (release years, album names, well-known collaborations).",
            "medium" => "Include a mix of popular songs and well-known album tracks. Use more detailed facts (chart positions, awards, movie soundtracks). Stick to established artists with confirmed Spotify presence.",
            "hard" => "Include more challenging questions about music history, band lineups, and detailed discography facts. Still use real, verifiable songs that exist on Spotify. Avoid extremely obscure tracks.",
            _ => "Use popular songs from well-known artists that are widely available on Spotify."
        };

        return $@"Create a music quiz based on this prompt: ""{request.Prompt}""

Difficulty guidance: {difficultyGuidance}

EXAMPLE QUESTION TYPES TO USE:
✅ ""In what year did Taylor Swift release this Grammy-winning song?""
✅ ""Which Coldplay album features this hit single from 2008?""  
✅ ""Who performed this Billboard #1 hit from 1995?""
✅ ""This Eminem song was featured in which movie soundtrack?""
✅ ""Which band member wrote this Fleetwood Mac classic?""
✅ ""This song by The Weeknd reached what peak position on the Billboard Hot 100?""

AVOID THESE PATTERNS:
❌ ""What song contains the lyrics 'I want it that way'?""
❌ ""Complete this lyric from Bohemian Rhapsody: 'Is this the real ___'?""
❌ ""What Backstreet Boys song starts with 'You are my fire'?""

CRITICAL REQUIREMENTS:
- ALL songs MUST be real tracks available on Spotify
- NEVER include song titles or obvious lyrics in question text
- Create diverse question types (release years, albums, chart positions, collaborations, awards, soundtracks, etc.)
- Use EXACT artist names and song titles as they appear on Spotify  
- Prioritize mainstream, well-known tracks over obscure ones
- Verify track/artist combinations are correct (no made-up pairings)
- Use primary artist names (main performer, not featuring artists)
- Use official track titles (no remixes, live versions, or alternate titles unless specified)

Make sure to:
- Use real, verifiable songs and artists
- Provide accurate track and artist names for Spotify lookup
- Make questions engaging and educational
- Include variety in question types (song identification, artist facts, album info, etc.)
- Ensure all answer options are plausible

Respond only with valid JSON in the specified format.";
    }

    private bool ValidateQuestionQuality(GeneratedQuestion question)
    {
        if (question?.QuestionText == null || question.TrackInfo?.TrackName == null)
            return false;

        var questionText = question.QuestionText.ToLowerInvariant();
        var trackName = question.TrackInfo.TrackName.ToLowerInvariant();
        var artistName = question.TrackInfo.ArtistName?.ToLowerInvariant() ?? "";

        // Forbidden patterns that give away the answer
        var forbiddenPatterns = new[]
        {
            "what song contains the lyrics",
            "complete this lyric",
            "which song starts with",
            "this lyric is from",
            "what song has the lyric",
            "fill in the blank",
            "what are the next lyrics",
            "which song includes the lyrics",
            "what song features the line"
        };

        // Check if question contains forbidden patterns
        foreach (var pattern in forbiddenPatterns)
        {
            if (questionText.Contains(pattern))
                return false;
        }

        // Check if the track title appears in the question (major giveaway)
        var trackWords = trackName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (trackWords.Length > 1)
        {
            // Check if a significant portion of the track name appears in question
            var matchingWords = trackWords.Count(word => 
                word.Length > 2 && questionText.Contains(word));
            
            if (matchingWords >= trackWords.Length / 2)
                return false;
        }
        else if (trackWords.Length == 1 && trackWords[0].Length > 3)
        {
            // Single word track name
            if (questionText.Contains(trackWords[0]))
                return false;
        }

        // Check if artist name appears in question when it shouldn't
        if (!string.IsNullOrEmpty(artistName))
        {
            var artistWords = artistName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in artistWords)
            {
                if (word.Length > 3 && questionText.Contains(word))
                {
                    // Allow artist name if it's asking about the artist specifically
                    if (!questionText.Contains("who") && !questionText.Contains("which artist"))
                        return false;
                }
            }
        }

        // Ensure question has good educational value
        var goodPatterns = new[]
        {
            "what year",
            "which album",
            "what album",
            "which movie", 
            "what soundtrack",
            "who performed",
            "who sang",
            "which band",
            "what position",
            "how many",
            "which chart",
            "what award",
            "which grammy",
            "what genre",
            "who wrote",
            "who produced",
            "which label",
            "what decade"
        };

        var hasGoodPattern = goodPatterns.Any(pattern => questionText.Contains(pattern));
        return hasGoodPattern;
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

            var quizData = JsonSerializer.Deserialize<GeneratedQuizData>(cleanedResponse, options);
            
            // NOTE: Temporarily skip question quality filtering to retain full AI output for debugging.

            return quizData;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI response as JSON: {Response}", aiResponse);
            return null;
        }
    }

    private async Task<Track?> FindSpotifyTrackWithRetry(string trackName, string artistName)
    {
        try
        {
            _logger.LogInformation("Looking for track with retry: {Artist} - {Track}", artistName, trackName);

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

            // Step 2: Multiple search strategies for Spotify API
            var searchStrategies = new[]
            {
                "Exact",
                "Standard", 
                "Simplified",
                "Fuzzy"
            };

            Track? spotifyTrack = null;
            string? successfulStrategy = null;

            foreach (var strategy in searchStrategies)
            {
                try
                {
                    _logger.LogInformation("Trying search strategy '{Strategy}' for {Artist} - {Track}", strategy, artistName, trackName);
                    
                    // Apply strategy-specific modifications to search terms
                    var (searchTrackName, searchArtistName, allowPartialMatch) = ApplySearchStrategy(strategy, trackName, artistName);
                    
                    spotifyTrack = await _spotifyApiService.SearchTrackAsync(searchTrackName, searchArtistName, allowPartialMatch);
                    
                    if (spotifyTrack != null)
                    {
                        successfulStrategy = strategy;
                        _logger.LogInformation("Strategy '{Strategy}' found track: {SpotifyId}", strategy, spotifyTrack.SpotifyTrackId);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Strategy '{Strategy}' failed for {Artist} - {Track}", strategy, artistName, trackName);
                }
            }
            
            if (spotifyTrack != null)
            {
                // Step 3: Check if this Spotify track already exists in database
                var existingBySpotifyId = await _unitOfWork.Tracks.GetBySpotifyTrackIdAsync(spotifyTrack.SpotifyTrackId);
                if (existingBySpotifyId != null)
                {
                    _logger.LogInformation("Found existing Spotify track in database: {SpotifyId} (found via {Strategy})", 
                        spotifyTrack.SpotifyTrackId, successfulStrategy);
                    return existingBySpotifyId;
                }

                // Step 4: Save new track to database
                var savedTrack = await _unitOfWork.Tracks.AddAsync(spotifyTrack);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Saved new Spotify track: {SpotifyId} - {Artist} - {Track} (found via {Strategy})", 
                    savedTrack.SpotifyTrackId, savedTrack.ArtistName, savedTrack.Name, successfulStrategy);
                
                return savedTrack;
            }

            // Step 5: All strategies failed - track not found
            _logger.LogWarning("All search strategies failed for {Artist} - {Track}, track not available on Spotify", artistName, trackName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding Spotify track with retry for {Artist} - {Track}", artistName, trackName);
            return null;
        }
    }

    private (string trackName, string artistName, bool allowPartialMatch) ApplySearchStrategy(string strategyName, string originalTrackName, string originalArtistName)
    {
        return strategyName switch
        {
            "Exact" => (originalTrackName, originalArtistName, false),
            "Standard" => (originalTrackName.Trim(), originalArtistName.Trim(), false),
            "Simplified" => (SimplifySearchTerm(originalTrackName), SimplifySearchTerm(originalArtistName), false),
            "Fuzzy" => (SimplifySearchTerm(originalTrackName), SimplifySearchTerm(originalArtistName), true),
            _ => (originalTrackName, originalArtistName, false)
        };
    }

    private string SimplifySearchTerm(string term)
    {
        if (string.IsNullOrEmpty(term))
            return term;
            
        // Remove common special characters that might interfere with search
        return System.Text.RegularExpressions.Regex.Replace(term, @"[^\w\s]", " ")
            .Trim()
            .Replace("  ", " "); // Replace double spaces with single space
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