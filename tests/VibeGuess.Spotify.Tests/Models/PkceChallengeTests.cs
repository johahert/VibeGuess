using VibeGuess.Spotify.Authentication.Models;

namespace VibeGuess.Spotify.Tests.Models;

public class PkceChallengeTests
{
    [Fact]
    public void IsExpired_WhenExpiresAtIsFuture_ReturnsFalse()
    {
        // Arrange
        var challenge = new PkceChallenge
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Act & Assert
        Assert.False(challenge.IsExpired);
    }

    [Fact]
    public void IsExpired_WhenExpiresAtIsPast_ReturnsTrue()
    {
        // Arrange
        var challenge = new PkceChallenge
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };

        // Act & Assert
        Assert.True(challenge.IsExpired);
    }

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Act
        var challenge = new PkceChallenge();

        // Assert
        Assert.True(challenge.CreatedAt <= DateTime.UtcNow);
        Assert.True(challenge.ExpiresAt > DateTime.UtcNow);
        Assert.Equal(string.Empty, challenge.CodeVerifier);
        Assert.Equal(string.Empty, challenge.CodeChallenge);
        Assert.Equal(string.Empty, challenge.State);
    }

    [Fact]
    public void ExpiresAt_DefaultsTo10MinutesFromCreation()
    {
        // Act
        var challenge = new PkceChallenge();
        var expectedExpiry = challenge.CreatedAt.AddMinutes(10);

        // Assert - Allow 1 second tolerance for test execution time
        Assert.True(Math.Abs((challenge.ExpiresAt - expectedExpiry).TotalSeconds) < 1);
    }
}