using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VibeGuess.Api.Services.Authentication;
using VibeGuess.Core.Entities;
using VibeGuess.Spotify.Authentication.Models;
using VibeGuess.Spotify.Authentication.Services;

namespace VibeGuess.Api.Services.Spotify;

/// <summary>
/// Implementation of Spotify Web API service for searching and retrieving tracks.
/// </summary>
public class SpotifyApiService : ISpotifyApiService
{
    private readonly HttpClient _httpClient;
    private readonly ISpotifyAuthenticationService _spotifyAuth;
    private readonly ICurrentUserSpotifyService _currentUserService;
    private readonly SpotifyAuthenticationOptions _options;
    private readonly ILogger<SpotifyApiService> _logger;

    public SpotifyApiService(
        IHttpClientFactory httpClientFactory,
        ISpotifyAuthenticationService spotifyAuth,
        ICurrentUserSpotifyService currentUserService,
        IOptions<SpotifyAuthenticationOptions> options,
        ILogger<SpotifyApiService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Spotify");
        _spotifyAuth = spotifyAuth;
        _currentUserService = currentUserService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Track?> SearchTrackAsync(string trackName, string artistName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(trackName) || string.IsNullOrWhiteSpace(artistName))
        {
            _logger.LogWarning("Invalid search parameters: trackName={TrackName}, artistName={ArtistName}", trackName, artistName);
            return null;
        }

        try
        {
            // Get access token - prioritize user authentication over client credentials
            var (accessToken, isUserToken) = await GetAccessTokenAsync(cancellationToken);
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Failed to get Spotify access token for track search - API integration not available");
                return null;
            }

            _logger.LogDebug("Using {TokenType} for track search", isUserToken ? "user token" : "client credentials");

            // Construct search query with proper escaping
            var query = Uri.EscapeDataString($"track:\"{trackName}\" artist:\"{artistName}\"");
            var requestUri = $"{_options.ApiBaseUrl}/search?q={query}&type=track&limit=1";

            // Set authorization header using AuthenticationHeaderValue
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            _logger.LogInformation("Searching Spotify for track: {Artist} - {Track}", artistName, trackName);

            var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            // Handle different HTTP status codes appropriately
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    break;
                case System.Net.HttpStatusCode.TooManyRequests:
                    _logger.LogWarning("Spotify API rate limit exceeded for search: {Artist} - {Track}", artistName, trackName);
                    return null;
                case System.Net.HttpStatusCode.Unauthorized:
                    _logger.LogWarning("Spotify API authentication failed for search: {Artist} - {Track}", artistName, trackName);
                    return null;
                case System.Net.HttpStatusCode.BadRequest:
                    _logger.LogWarning("Spotify API bad request for search: {Artist} - {Track}", artistName, trackName);
                    return null;
                default:
                    _logger.LogWarning("Spotify search failed with status {StatusCode} for {Artist} - {Track}", 
                        response.StatusCode, artistName, trackName);
                    return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (string.IsNullOrEmpty(responseContent))
            {
                _logger.LogWarning("Empty response from Spotify API for {Artist} - {Track}", artistName, trackName);
                return null;
            }

            var searchResult = JsonSerializer.Deserialize<SpotifySearchResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (searchResult?.Tracks?.Items == null || !searchResult.Tracks.Items.Any())
            {
                _logger.LogInformation("No Spotify tracks found for {Artist} - {Track}", artistName, trackName);
                return null;
            }

            var spotifyTrack = searchResult.Tracks.Items.First();
            var track = MapSpotifyTrackToEntity(spotifyTrack);

            _logger.LogInformation("Found Spotify track: {SpotifyId} - {Artist} - {Track}", 
                track.SpotifyTrackId, track.ArtistName, track.Name);

            return track;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("Spotify API request timeout for {Artist} - {Track}", artistName, trackName);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Spotify API network error for {Artist} - {Track}", artistName, trackName);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Spotify API response for {Artist} - {Track}", artistName, trackName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error searching Spotify for track {Artist} - {Track}", artistName, trackName);
            return null;
        }
    }

    public async Task<Track?> GetTrackAsync(string spotifyTrackId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get access token - prioritize user authentication over client credentials
            var (accessToken, isUserToken) = await GetAccessTokenAsync(cancellationToken);
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Failed to get Spotify access token for track details");
                return null;
            }

            _logger.LogDebug("Using {TokenType} for track retrieval", isUserToken ? "user token" : "client credentials");

            var requestUri = $"{_options.ApiBaseUrl}/tracks/{spotifyTrackId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Spotify get track failed with status {StatusCode} for track {SpotifyTrackId}", 
                    response.StatusCode, spotifyTrackId);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var spotifyTrack = JsonSerializer.Deserialize<SpotifyTrack>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (spotifyTrack == null)
            {
                return null;
            }

            return MapSpotifyTrackToEntity(spotifyTrack);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Spotify track {SpotifyTrackId}", spotifyTrackId);
            return null;
        }
    }

    /// <summary>
    /// Gets an access token, prioritizing user authentication over client credentials.
    /// Returns the token and a flag indicating whether it's a user token.
    /// </summary>
    private async Task<(string? accessToken, bool isUserToken)> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            // First, try to get user's Spotify token if they are authenticated
            var userToken = await _currentUserService.GetCurrentUserSpotifyTokenAsync();
            if (!string.IsNullOrEmpty(userToken))
            {
                _logger.LogDebug("Using authenticated user's Spotify token for API request");
                return (userToken, true);
            }

            // Fall back to client credentials if no user is authenticated
            _logger.LogDebug("No user authentication found, falling back to client credentials");
            var clientToken = await GetClientCredentialsTokenAsync(cancellationToken);
            return (clientToken, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting access token");
            return (null, false);
        }
    }

    private async Task<string?> GetClientCredentialsTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(_options.ClientId) || string.IsNullOrEmpty(_options.ClientSecret))
            {
                _logger.LogWarning("Spotify client credentials not configured properly");
                return null;
            }

            _logger.LogInformation("Attempting Spotify client credentials authentication with ClientId: {ClientId}", _options.ClientId);

            using var httpClient = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            
            // Set Basic authentication header using AuthenticationHeaderValue
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

            // Create the form data for client credentials
            var postData = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials")
            };
            request.Content = new FormUrlEncodedContent(postData);

            _logger.LogInformation("Making POST request to Spotify token endpoint");

            var response = await httpClient.SendAsync(request, cancellationToken);

            _logger.LogInformation("Spotify token response: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to get Spotify client credentials token: {StatusCode} - {Error}", 
                    response.StatusCode, errorContent);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (tokenResponse?.AccessToken == null)
            {
                _logger.LogError("Invalid token response from Spotify API");
                return null;
            }

            _logger.LogInformation("Successfully obtained Spotify client credentials token");
            return tokenResponse.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Spotify client credentials token");
            return null;
        }
    }

    private static Track MapSpotifyTrackToEntity(SpotifyTrack spotifyTrack)
    {
        return new Track
        {
            Id = Guid.NewGuid(),
            SpotifyTrackId = spotifyTrack.Id,
            Name = spotifyTrack.Name,
            ArtistName = spotifyTrack.Artists?.FirstOrDefault()?.Name ?? "Unknown Artist",
            AllArtists = string.Join(", ", spotifyTrack.Artists?.Select(a => a.Name) ?? new[] { "Unknown Artist" }),
            AlbumName = spotifyTrack.Album?.Name ?? "Unknown Album",
            DurationMs = spotifyTrack.DurationMs,
            Popularity = spotifyTrack.Popularity,
            IsExplicit = spotifyTrack.Explicit,
            PreviewUrl = spotifyTrack.PreviewUrl,
            SpotifyUrl = spotifyTrack.ExternalUrls?.Spotify,
            AlbumImageUrl = spotifyTrack.Album?.Images?.FirstOrDefault()?.Url,
            ReleaseDate = DateTime.TryParse(spotifyTrack.Album?.ReleaseDate, out var date) ? date : null,
            CreatedAt = DateTime.UtcNow
        };
    }
}

// Spotify API response models
public class SpotifySearchResponse
{
    public SpotifyTracksResponse? Tracks { get; set; }
}

public class SpotifyTracksResponse
{
    public List<SpotifyTrack> Items { get; set; } = new();
    public int Total { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
}

public class SpotifyTrack
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<SpotifyArtist>? Artists { get; set; }
    public SpotifyAlbum? Album { get; set; }
    public int DurationMs { get; set; }
    public int Popularity { get; set; }
    public bool Explicit { get; set; }
    public string? PreviewUrl { get; set; }
    public SpotifyExternalUrls? ExternalUrls { get; set; }
}

public class SpotifyArtist
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class SpotifyAlbum
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<SpotifyImage>? Images { get; set; }
    public string? ReleaseDate { get; set; }
}

public class SpotifyImage
{
    public string Url { get; set; } = string.Empty;
    public int Height { get; set; }
    public int Width { get; set; }
}

public class SpotifyExternalUrls
{
    public string? Spotify { get; set; }
}

public class SpotifyTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}