using groove_glass_api.Models;
using groove_glass_api.Models.Frontend;
using groove_glass_api.Services.Interfaces;
using OpenAI.Chat;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace groove_glass_api.Services.Implementations
{
    public class SpotifyApiService : ISpotifyApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        private const string SPOTIFY_TOKEN_URL = "https://accounts.spotify.com/api/token";
        private const string SPOTIFY_BASE_URL = "https://api.spotify.com/v1";

        public SpotifyApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<SpotifyUserProfileResponse> ExchangeCodeAndGetProfileAsync(string code)
        {
            // Get access token
            var tokenResponse = await GetAccessTokenResponseAsync(code);

            var client = _httpClientFactory.CreateClient();

            // Get user profile
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
            var profileResponse = await client.GetAsync($"{SPOTIFY_BASE_URL}/me");
            if (!profileResponse.IsSuccessStatusCode)
            {
                var errorContent = await profileResponse.Content.ReadAsStringAsync();
                throw new Exception($"Spotify profile error: {errorContent}");
            }

            var profileString = await profileResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(profileString);
            var root = doc.RootElement;
            var userId = root.GetProperty("id").GetString();
            var displayName = root.TryGetProperty("display_name", out var dn) ? dn.GetString() : null;

            return new SpotifyUserProfileResponse
            {
                Token = tokenResponse,
                SpotifyUserId = userId,
                DisplayName = displayName
            };
        }

        public async Task<SpotifyTokenResponse> GetAccessTokenResponseAsync(string code)
        {
            var clientId = _configuration["Spotify:ClientId"];
            var clientSecret = _configuration["Spotify:ClientSecret"];
            var redirectUri = _configuration["Spotify:RedirectUri"];
            var client = _httpClientFactory.CreateClient();
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            var requestBody = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri }
            };
            var content = new FormUrlEncodedContent(requestBody);
            var response = await client.PostAsync(SPOTIFY_TOKEN_URL, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Spotify token error: {errorContent}");
            }
            var responseString = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(responseString);

            if (tokenResponse == null)
            {
                throw new Exception("Failed to deserialize Spotify token response.");
            }

            return tokenResponse;
        }

        public Task<string> GetUserProfileAsync(string accessToken)
        {
            throw new NotImplementedException();
        }

        public async Task<SpotifyTokenResponse> RefreshAccessTokenAsync(string refreshToken)
        {
            var clientId = _configuration["Spotify:ClientId"];
            var clientSecret = _configuration["Spotify:ClientSecret"];
            var tokenEndpoint = "https://accounts.spotify.com/api/token";

            using var httpClient = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var postData = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "refresh_token"),
                new("refresh_token", refreshToken)
            };
            request.Content = new FormUrlEncodedContent(postData);

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Spotify refresh token error: {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(content);

            return tokenResponse ?? throw new Exception("Failed to deserialize Spotify token response.");
        }

        public async Task<List<SpotifyTrackResult>> SearchTracksAsync(string query, string accessToken, int limit)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var searchUrl = $"{SPOTIFY_BASE_URL}/search?q={Uri.EscapeDataString(query)}&type=track&limit={limit}";
            var response = await client.GetAsync(searchUrl);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Spotify search error: {errorContent}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;
            var tracks = root.GetProperty("tracks").GetProperty("items");
            var trackList = new List<SpotifyTrackResult>();

            foreach (var track in tracks.EnumerateArray())
            {
                var trackName = track.GetProperty("name").GetString();
                var trackId = track.GetProperty("id").GetString();
                var artists = track.GetProperty("artists").EnumerateArray()
                    .Select(a => a.GetProperty("name").GetString()).ToList();
                var artistNames = string.Join(", ", artists);
                trackList.Add(new SpotifyTrackResult
                {
                    Title = trackName,
                    Id = trackId,
                    Artists = artists
                });
            }
            return trackList;
        }

        public Task<bool> PlayTrackAsync(string trackId, string deviceId, string accessToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var playUrl = $"{SPOTIFY_BASE_URL}/me/player/play?device_id={deviceId}";
                var requestBody = new
                {
                    uris = new[] { $"spotify:track:{trackId}" }
                };
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = client.PutAsync(playUrl, content).Result;
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = response.Content.ReadAsStringAsync().Result;
                    throw new Exception($"Spotify play track error: {errorContent}");
                }
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Task.FromResult(false);
            }

        }

        public async Task<List<SpotifyDeviceResult>> GetAvailableDevicesAsync(string accessToken)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync("https://api.spotify.com/v1/me/player/devices");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;
            var devices = root.GetProperty("devices");

            var results = new List<SpotifyDeviceResult>();
            foreach (var device in devices.EnumerateArray())
            {
                results.Add(new SpotifyDeviceResult
                {
                    Id = device.GetProperty("id").GetString(),
                    Name = device.GetProperty("name").GetString(),
                    Type = device.GetProperty("type").GetString(),
                    IsActive = device.GetProperty("is_active").GetBoolean()
                });
            }
            return results;
        }

        public async Task<bool> PauseTrackAsync(string deviceId, string accessToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var pauseUrl = $"{SPOTIFY_BASE_URL}/me/player/pause?device_id={deviceId}";
                var response = await client.PutAsync(pauseUrl, null);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Spotify pause track error: {errorContent}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
