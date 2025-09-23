namespace VibeGuess.Api.Services.OpenAI;

public class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4";
    public string Endpoint { get; set; } = "https://api.openai.com";
}