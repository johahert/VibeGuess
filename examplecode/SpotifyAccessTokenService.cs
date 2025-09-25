using groove_glass_api.Models;
using groove_glass_api.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace groove_glass_api.Services.Implementations
{
    public class SpotifyAccessTokenService : AccessTokenService<SpotifyTokenResponse>
    {
        private const string SPOTIFY_TOKEN_URL = "https://accounts.spotify.com/api/token";
        private const string SPOTIFY_BASE_URL = "https://api.spotify.com/v1";

        public SpotifyAccessTokenService(IConfiguration configuration, IHttpClientFactory httpClientFactory) : base(configuration, httpClientFactory)
        {
        }

        public async Task<SpotifyUserProfileResponse> ExchangeCodeAndGetProfileAsync(string code)
        {
            // Get access token
            var tokenResponse = await GetAccessTokensAsync(code);

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

        public override async Task<SpotifyTokenResponse> RefreshAccessTokensAsync(string refreshToken)
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

        protected override async Task<SpotifyTokenResponse> GetAccessTokensAsync(string code)
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
    }
}
