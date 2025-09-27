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

    public async Task<IReadOnlyList<Track>?> SearchTracksAsync(string query, int limit = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            _logger.LogWarning("Invalid search query provided");
            return Array.Empty<Track>();
        }

        var normalizedLimit = Math.Clamp(limit, 1, 10);

        try
        {
            var (accessToken, isUserToken) = await GetAccessTokenAsync(cancellationToken);
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Failed to get Spotify access token for query search");
                return null;
            }

            _logger.LogDebug("Using {TokenType} for query search", isUserToken ? "user token" : "client credentials");

            var requestUri = $"{_options.ApiBaseUrl}/search?q={Uri.EscapeDataString(query)}&type=track&limit={normalizedLimit}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            _logger.LogInformation("Searching Spotify for tracks with query: {Query} (limit {Limit})", query, normalizedLimit);

            var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    break;
                case System.Net.HttpStatusCode.TooManyRequests:
                    _logger.LogWarning("Spotify API rate limit exceeded for query search: {Query}", query);
                    return Array.Empty<Track>();
                case System.Net.HttpStatusCode.Unauthorized:
                case System.Net.HttpStatusCode.Forbidden:
                    _logger.LogWarning("Spotify API authentication failed for query search: {Query}", query);
                    return null;
                case System.Net.HttpStatusCode.BadRequest:
                    _logger.LogWarning("Spotify API bad request for query search: {Query}", query);
                    return Array.Empty<Track>();
                default:
                    _logger.LogWarning("Spotify query search failed with status {StatusCode} for query: {Query}", response.StatusCode, query);
                    return Array.Empty<Track>();
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrEmpty(responseContent))
            {
                _logger.LogWarning("Empty response from Spotify API for query search: {Query}", query);
                return Array.Empty<Track>();
            }

            var searchResult = JsonSerializer.Deserialize<SpotifySearchResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (searchResult?.Tracks?.Items == null || !searchResult.Tracks.Items.Any())
            {
                _logger.LogInformation("No Spotify tracks found for query: {Query}", query);
                return Array.Empty<Track>();
            }

            var tracks = searchResult.Tracks.Items
                .Where(item => item != null)
                .Select(MapSpotifyTrackToEntity)
                .ToArray();

            _logger.LogInformation("Found {Count} Spotify tracks for query: {Query}", tracks.Length, query);
            return tracks;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("Spotify API request timeout for query search: {Query}", query);
            return Array.Empty<Track>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Spotify API network error for query search: {Query}", query);
            return Array.Empty<Track>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Spotify API response for query search: {Query}", query);
            return Array.Empty<Track>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error searching Spotify for query: {Query}", query);
            return Array.Empty<Track>();
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

    public async Task<IEnumerable<SpotifyDevice>?> GetUserDevicesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // This endpoint requires user authentication - client credentials won't work
            var userToken = await _currentUserService.GetCurrentUserSpotifyTokenAsync();
            if (string.IsNullOrEmpty(userToken))
            {
                _logger.LogWarning("No user authentication available for device retrieval - user must be logged in");
                return null;
            }

            _logger.LogInformation("Retrieving Spotify devices for authenticated user");

            var requestUri = $"{_options.ApiBaseUrl}/me/player/devices";

            // Clear any existing headers and set user's access token
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            // Handle different HTTP status codes appropriately
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    _logger.LogWarning("Spotify user token is invalid or expired for device retrieval");
                    return null;
                case System.Net.HttpStatusCode.Forbidden:
                    _logger.LogWarning("Spotify user lacks required scopes for device access");
                    return null;
                case System.Net.HttpStatusCode.TooManyRequests:
                    _logger.LogWarning("Spotify API rate limit exceeded for device retrieval");
                    return null;
                case System.Net.HttpStatusCode.NoContent:
                    // No devices available - this is normal
                    _logger.LogInformation("No Spotify devices found for user");
                    return new List<SpotifyDevice>();
                default:
                    _logger.LogWarning("Spotify device retrieval failed with status {StatusCode}", response.StatusCode);
                    return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (string.IsNullOrEmpty(responseContent))
            {
                _logger.LogInformation("Empty response from Spotify devices API - no devices available");
                return new List<SpotifyDevice>();
            }

            var devicesResponse = JsonSerializer.Deserialize<SpotifyDevicesResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (devicesResponse?.Devices == null)
            {
                _logger.LogInformation("No devices found in Spotify API response");
                return new List<SpotifyDevice>();
            }

            _logger.LogInformation("Retrieved {DeviceCount} Spotify devices for user", devicesResponse.Devices.Count);
            
            // Log device details for debugging
            foreach (var device in devicesResponse.Devices)
            {
                _logger.LogDebug("Found device: {Name} ({Type}) - Active: {IsActive}, ID: {DeviceId}", 
                    device.Name, device.Type, device.IsActive, device.Id);
            }

            return devicesResponse.Devices;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("Spotify devices API request timeout");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Spotify devices API network error");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Spotify devices API response");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving Spotify devices");
            return null;
        }
    }

    public async Task<bool> StartPlaybackAsync(string? deviceId = null, string[]? trackUris = null, string? contextUri = null, int? positionMs = null, CancellationToken cancellationToken = default)
    {
        var (accessToken, _) = await GetAccessTokenAsync(CancellationToken.None);
        if (string.IsNullOrEmpty(accessToken))
        {
            throw new InvalidOperationException("No valid Spotify access token available");
        }

        using var client = new HttpClient();
        
        var url = $"{_options.ApiBaseUrl}/me/player/play";
        if (!string.IsNullOrEmpty(deviceId))
        {
            url += $"?device_id={Uri.EscapeDataString(deviceId)}";
        }

        var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var payload = new Dictionary<string, object>();
        if (trackUris?.Length > 0)
        {
            payload["uris"] = trackUris;
        }
        if (!string.IsNullOrEmpty(contextUri))
        {
            payload["context_uri"] = contextUri;
        }
        if (positionMs.HasValue)
        {
            payload["position_ms"] = positionMs.Value;
        }

        if (payload.Any())
        {
            var json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("No active device found. Please start Spotify on a device first.");
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("Spotify Premium subscription required for playback control");
            }

            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("Started Spotify playback successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting Spotify playback");
            throw;
        }
    }

    public async Task<bool> PausePlaybackAsync(string? deviceId = null, CancellationToken cancellationToken = default)
    {
        var (accessToken, _) = await GetAccessTokenAsync(CancellationToken.None);
        if (string.IsNullOrEmpty(accessToken))
        {
            throw new InvalidOperationException("No valid Spotify access token available");
        }

        using var client = new HttpClient();
        
        var url = $"{_options.ApiBaseUrl}/me/player/pause";
        if (!string.IsNullOrEmpty(deviceId))
        {
            url += $"?device_id={Uri.EscapeDataString(deviceId)}";
        }

        var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("No active device found. Please start Spotify on a device first.");
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("Spotify Premium subscription required for playback control");
            }

            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("Paused Spotify playback successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing Spotify playback");
            throw;
        }
    }

    public async Task<SpotifyPlaybackState?> GetCurrentPlaybackAsync(string? market = "US", CancellationToken cancellationToken = default)
    {
        var (accessToken, _) = await GetAccessTokenAsync(CancellationToken.None);
        if (string.IsNullOrEmpty(accessToken))
        {
            throw new InvalidOperationException("No valid Spotify access token available");
        }

        using var client = new HttpClient();
        
        var url = $"{_options.ApiBaseUrl}/me/player?market={Uri.EscapeDataString(market ?? "US")}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                // No active session or device
                return null;
            }

            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var playbackState = JsonSerializer.Deserialize<SpotifyPlaybackState>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Retrieved current Spotify playback state successfully");
            return playbackState;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current Spotify playback state");
            throw;
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

public class SpotifyDevicesResponse
{
    public List<SpotifyDevice> Devices { get; set; } = new();
}

public class SpotifyDevice
{
    public string Id { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsPrivateSession { get; set; }
    public bool IsRestricted { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int? VolumePercent { get; set; }
}

public class SpotifyDeviceType
{
    public const string Computer = "Computer";
    public const string Tablet = "Tablet"; 
    public const string Smartphone = "Smartphone";
    public const string Speaker = "Speaker";
    public const string TV = "TV";
    public const string AVR = "AVR";
    public const string STB = "STB";
    public const string AudioDongle = "AudioDongle";
    public const string GameConsole = "GameConsole";
    public const string CastVideo = "CastVideo";
    public const string CastAudio = "CastAudio";
    public const string Automobile = "Automobile";
    public const string Unknown = "Unknown";
}

public class SpotifyPlaybackState
{
    public bool IsPlaying { get; set; }
    public long Timestamp { get; set; }
    public int? ProgressMs { get; set; }
    public SpotifyTrack? Item { get; set; }
    public SpotifyDevice? Device { get; set; }
    public bool ShuffleState { get; set; }
    public string RepeatState { get; set; } = "off"; // "off", "track", "context"
    public SpotifyContext? Context { get; set; }
    public SpotifyActions? Actions { get; set; }
}

public class SpotifyContext
{
    public string Type { get; set; } = string.Empty; // "playlist", "album", "artist"
    public string Href { get; set; } = string.Empty;
    public string Uri { get; set; } = string.Empty;
}

public class SpotifyActions
{
    public bool InterruptingPlayback { get; set; }
    public bool Pausing { get; set; }
    public bool Resuming { get; set; }
    public bool Seeking { get; set; }
    public bool SkippingNext { get; set; }
    public bool SkippingPrev { get; set; }
    public bool TogglingRepeatContext { get; set; }
    public bool TogglingRepeatTrack { get; set; }
    public bool TogglingShuffle { get; set; }
    public bool TransferringPlayback { get; set; }
    public SpotifyDisallows? Disallows { get; set; }
}

public class SpotifyDisallows
{
    public bool InterruptingPlayback { get; set; }
    public bool Pausing { get; set; }
    public bool Resuming { get; set; }
    public bool Seeking { get; set; }
    public bool SkippingNext { get; set; }
    public bool SkippingPrev { get; set; }
    public bool TogglingRepeatContext { get; set; }
    public bool TogglingRepeatTrack { get; set; }
    public bool TogglingShuffle { get; set; }
    public bool TransferringPlayback { get; set; }
}