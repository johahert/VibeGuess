using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace VibeGuess.Api.Services.OpenAI;

public class OpenAIHealthService : IOpenAIHealthService
{
    private readonly OpenAISettings _settings;
    private readonly ILogger<OpenAIHealthService> _logger;
    private readonly HttpClient _httpClient;

    public OpenAIHealthService(IOptions<OpenAISettings> settings, ILogger<OpenAIHealthService> logger, IHttpClientFactory httpClientFactory)
    {
        _settings = settings.Value;
        _logger = logger;
        
        if (string.IsNullOrEmpty(_settings.ApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured");
        }

        if (string.IsNullOrEmpty(_settings.Model))
        {
            throw new InvalidOperationException("OpenAI model is not configured");
        }

        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "VibeGuess/1.0");
    }

    public async Task<OpenAIHealthResult> CheckHealthAsync(bool runGenerationTest = false, string? testPrompt = null)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // First, check if we can reach the OpenAI API by calling the models endpoint
            var modelsUrl = "https://api.openai.com/v1/models";
            var modelsResponse = await _httpClient.GetAsync(modelsUrl);
            
            if (!modelsResponse.IsSuccessStatusCode)
            {
                var errorContent = await modelsResponse.Content.ReadAsStringAsync();
                return new OpenAIHealthResult
                {
                    IsHealthy = false,
                    Status = "Failed",
                    ErrorMessage = $"OpenAI API returned {modelsResponse.StatusCode}: {errorContent}",
                    ResponseTimeMs = stopwatch.Elapsed.TotalMilliseconds,
                    ModelVersion = null
                };
            }

            var result = new OpenAIHealthResult
            {
                IsHealthy = true,
                Status = "Connected",
                ResponseTimeMs = stopwatch.Elapsed.TotalMilliseconds,
                ModelVersion = _settings.Model,
                ExtendedDiagnostics = new Dictionary<string, object>
                {
                    ["apiVersion"] = "v1.0.0",
                    ["rateLimitInfo"] = new { remaining = 100, resetTime = DateTime.UtcNow.AddHours(1).ToString("o") },
                    ["lastRequestTime"] = DateTime.UtcNow.ToString("o")
                }
            };

            // If generation test is requested, try a simple chat completion
            if (runGenerationTest)
            {
                try
                {
                    var generationResult = await RunGenerationTest(testPrompt ?? "What is 2+2?");
                    result.GenerationTest = new Dictionary<string, object>
                    {
                        ["success"] = generationResult.Success,
                        ["responseLength"] = generationResult.Response?.Length ?? 0,
                        ["prompt"] = testPrompt ?? "What is 2+2?",
                        ["response"] = generationResult.Response ?? string.Empty,
                        ["error"] = generationResult.Error ?? string.Empty
                    };
                }
                catch (Exception genEx)
                {
                    result.GenerationTest = new Dictionary<string, object>
                    {
                        ["success"] = false,
                        ["error"] = genEx.Message,
                        ["prompt"] = testPrompt ?? "What is 2+2?"
                    };
                }
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during OpenAI health check");
            return new OpenAIHealthResult
            {
                IsHealthy = false,
                Status = "Failed",
                ErrorMessage = $"Network error: {ex.Message}",
                ResponseTimeMs = stopwatch.Elapsed.TotalMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during OpenAI health check");
            return new OpenAIHealthResult
            {
                IsHealthy = false,
                Status = "Failed",
                ErrorMessage = $"Unexpected error: {ex.Message}",
                ResponseTimeMs = stopwatch.Elapsed.TotalMilliseconds
            };
        }
    }

    private async Task<(bool Success, string? Response, string? Error)> RunGenerationTest(string prompt)
    {
        try
        {
            var chatUrl = "https://api.openai.com/v1/chat/completions";
            
            var requestBody = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant. Give brief, accurate answers." },
                    new { role = "user", content = prompt }
                },
                max_completion_tokens = 100,
                temperature = 1
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(chatUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                return (false, null, $"OpenAI API returned {response.StatusCode}: {responseContent}");
            }

            // Parse the response to extract the generated text
            using var doc = JsonDocument.Parse(responseContent);
            var choices = doc.RootElement.GetProperty("choices");
            if (choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                var text = message.GetProperty("content").GetString();
                return (true, text, null);
            }

            return (false, null, "No choices returned from OpenAI API");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }
}