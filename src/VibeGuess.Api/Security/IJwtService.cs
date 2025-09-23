using System.Security.Claims;

namespace VibeGuess.Api.Security;

public interface IJwtService
{
    string GenerateToken(string userId, string spotifyUserId, IEnumerable<Claim>? additionalClaims = null);
}
