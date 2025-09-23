namespace VibeGuess.Api.Security;

public class JwtSettings
{
    public string Issuer { get; set; } = "vibeguess";
    public string Audience { get; set; } = "vibeguess_clients";
    public string Secret { get; set; } = string.Empty; // MUST be set in configuration / env for production
    public int ExpiryMinutes { get; set; } = 60;
}
