namespace VibeGuess.Api.Services.OpenAI;

public interface IOpenAIHealthService
{
    Task<OpenAIHealthResult> CheckHealthAsync(bool runGenerationTest = false, string? testPrompt = null);
}

public class OpenAIHealthResult
{
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public double ResponseTimeMs { get; set; }
    public string? ModelVersion { get; set; }
    public Dictionary<string, object>? GenerationTest { get; set; }
    public Dictionary<string, object>? ExtendedDiagnostics { get; set; }
}