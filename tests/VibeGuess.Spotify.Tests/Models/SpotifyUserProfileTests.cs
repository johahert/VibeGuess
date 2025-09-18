using VibeGuess.Spotify.Authentication.Models;

namespace VibeGuess.Spotify.Tests.Models;

public class SpotifyUserProfileTests
{
    [Fact]
    public void HasPremium_WhenProductIsPremium_ReturnsTrue()
    {
        // Arrange
        var profile = new SpotifyUserProfile
        {
            Product = "premium"
        };

        // Act & Assert
        Assert.True(profile.HasPremium);
    }

    [Fact]
    public void HasPremium_WhenProductIsPremiumDifferentCase_ReturnsTrue()
    {
        // Arrange
        var profile = new SpotifyUserProfile
        {
            Product = "PREMIUM"
        };

        // Act & Assert
        Assert.True(profile.HasPremium);
    }

    [Fact]
    public void HasPremium_WhenProductIsFree_ReturnsFalse()
    {
        // Arrange
        var profile = new SpotifyUserProfile
        {
            Product = "free"
        };

        // Act & Assert
        Assert.False(profile.HasPremium);
    }

    [Fact]
    public void HasPremium_WhenProductIsNull_ReturnsFalse()
    {
        // Arrange
        var profile = new SpotifyUserProfile
        {
            Product = null!
        };

        // Act & Assert
        Assert.False(profile.HasPremium);
    }

    [Fact]
    public void ProfileImageUrl_WhenImagesExist_ReturnsFirstImageUrl()
    {
        // Arrange
        var profile = new SpotifyUserProfile
        {
            Images = new[]
            {
                new SpotifyImage { Url = "https://example.com/image1.jpg" },
                new SpotifyImage { Url = "https://example.com/image2.jpg" }
            }
        };

        // Act & Assert
        Assert.Equal("https://example.com/image1.jpg", profile.ProfileImageUrl);
    }

    [Fact]
    public void ProfileImageUrl_WhenNoImages_ReturnsNull()
    {
        // Arrange
        var profile = new SpotifyUserProfile
        {
            Images = Array.Empty<SpotifyImage>()
        };

        // Act & Assert
        Assert.Null(profile.ProfileImageUrl);
    }

    [Fact]
    public void ProfileImageUrl_WhenImagesIsNull_ReturnsNull()
    {
        // Arrange
        var profile = new SpotifyUserProfile
        {
            Images = null!
        };

        // Act & Assert
        Assert.Null(profile.ProfileImageUrl);
    }

    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        // Act
        var profile = new SpotifyUserProfile();

        // Assert
        Assert.Equal(string.Empty, profile.Id);
        Assert.Equal(string.Empty, profile.DisplayName);
        Assert.Equal(string.Empty, profile.Email);
        Assert.Equal(string.Empty, profile.Country);
        Assert.Equal(string.Empty, profile.Product);
        Assert.NotNull(profile.Images);
        Assert.Empty(profile.Images);
    }
}