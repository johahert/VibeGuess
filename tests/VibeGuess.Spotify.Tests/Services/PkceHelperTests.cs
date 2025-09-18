using VibeGuess.Spotify.Authentication.Services;
using VibeGuess.Spotify.Authentication.Models;

namespace VibeGuess.Spotify.Tests.Services;

public class PkceHelperTests
{
    [Fact]
    public void GenerateChallenge_ShouldReturnValidChallenge()
    {
        // Act
        var challenge = PkceHelper.GenerateChallenge();

        // Assert
        Assert.NotNull(challenge);
        Assert.NotEmpty(challenge.CodeVerifier);
        Assert.NotEmpty(challenge.CodeChallenge);
        Assert.NotEmpty(challenge.State);
        Assert.True(challenge.CreatedAt <= DateTime.UtcNow);
        Assert.True(challenge.ExpiresAt > DateTime.UtcNow);
        Assert.False(challenge.IsExpired);
    }

    [Fact]
    public void GenerateChallenge_ShouldReturnUniqueValues()
    {
        // Act
        var challenge1 = PkceHelper.GenerateChallenge();
        var challenge2 = PkceHelper.GenerateChallenge();

        // Assert
        Assert.NotEqual(challenge1.CodeVerifier, challenge2.CodeVerifier);
        Assert.NotEqual(challenge1.CodeChallenge, challenge2.CodeChallenge);
        Assert.NotEqual(challenge1.State, challenge2.State);
    }

    [Fact]
    public void GenerateChallenge_CodeVerifier_ShouldBe128Characters()
    {
        // Act
        var challenge = PkceHelper.GenerateChallenge();

        // Assert
        Assert.Equal(128, challenge.CodeVerifier.Length);
    }

    [Fact]
    public void GenerateChallenge_CodeChallenge_ShouldBe43Characters()
    {
        // Act
        var challenge = PkceHelper.GenerateChallenge();

        // Assert
        Assert.Equal(43, challenge.CodeChallenge.Length);
    }

    [Fact]
    public void GenerateChallenge_State_ShouldBe43Characters()
    {
        // Act
        var challenge = PkceHelper.GenerateChallenge();

        // Assert
        Assert.Equal(43, challenge.State.Length);
    }

    [Fact]
    public void GenerateChallenge_ShouldUseBase64UrlEncoding()
    {
        // Act
        var challenge = PkceHelper.GenerateChallenge();

        // Assert - Base64URL should not contain +, /, or = characters
        Assert.DoesNotContain("+", challenge.CodeVerifier);
        Assert.DoesNotContain("/", challenge.CodeVerifier);
        Assert.DoesNotContain("=", challenge.CodeVerifier);
        
        Assert.DoesNotContain("+", challenge.CodeChallenge);
        Assert.DoesNotContain("/", challenge.CodeChallenge);
        Assert.DoesNotContain("=", challenge.CodeChallenge);
        
        Assert.DoesNotContain("+", challenge.State);
        Assert.DoesNotContain("/", challenge.State);
        Assert.DoesNotContain("=", challenge.State);
    }
}