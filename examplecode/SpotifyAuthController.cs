using DatabaseService.Models.Entities;
using DatabaseService.Services.Interfaces;
using groove_glass_api.Models;
using groove_glass_api.Models.Frontend;
using groove_glass_api.Models.Frontend.QuizData;
using groove_glass_api.Services.Interfaces;
using groove_glass_api.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace groove_glass_api.Controllers
{
    [Route("api/spotify")]
    [ApiController]
    public class SpotifyAuthController : ControllerBase
    {
        private readonly ISpotifyApiService _spotifyApiService;
        private readonly EncryptionHelper _encryptionHelper;
        private readonly IAuthenticateSpotifyUserService _authenticateSpotifyUserService;
        private readonly IQuizStorageService _quizStorageService;
        private readonly IEntityStorageService<SpotifyUser, string> _userStorageService;

        public SpotifyAuthController(
            ISpotifyApiService spotifyApiService, 
            EncryptionHelper encryptionHelper, 
            IAuthenticateSpotifyUserService authenticateSpotifyUserService, 
            IQuizStorageService quizStorageService, 
            IEntityStorageService<SpotifyUser, string> userStorageService)
        {
            _spotifyApiService = spotifyApiService;
            _encryptionHelper = encryptionHelper;
            _authenticateSpotifyUserService = authenticateSpotifyUserService;
            _quizStorageService = quizStorageService;
            _userStorageService = userStorageService;
        }

        /// <summary>
        /// Stores the Spotify user profile and returns a JWT token.
        /// </summary>
        /// <param name="request">Access code</param>
        /// <returns>Spotify display name and jwt token</returns>
        [HttpPost("token")]
        public async Task<IActionResult> LoginSpotifyUser([FromBody] TokenRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
            {
                return BadRequest("Spotify authorization code missing");
            }

            try
            {
                var spotifyUserResponse = await _spotifyApiService.ExchangeCodeAndGetProfileAsync(request.Code);

                if (spotifyUserResponse == null)
                {
                    return NotFound("Spotify user not found with the provided code");
                }

                var jwtToken = _encryptionHelper.GenerateJwtToken(spotifyUserResponse.SpotifyUserId, spotifyUserResponse.DisplayName);
                var jwtRefreshToken = _encryptionHelper.GenerateJwtRefreshToken();
                var jwtTokenExpiration = _encryptionHelper.GetJwtTokenExpiration(jwtToken);

                var user = new SpotifyUser
                {
                    SpotifyUserId = spotifyUserResponse.SpotifyUserId,
                    DisplayName = spotifyUserResponse.DisplayName,
                    EncryptedAccessToken = _encryptionHelper.EncryptString(spotifyUserResponse.Token.AccessToken),
                    EncryptedRefreshToken = _encryptionHelper.EncryptString(spotifyUserResponse.Token.RefreshToken),
                    TokenExpiration = DateTime.UtcNow.AddSeconds(spotifyUserResponse.Token.ExpiresIn),
                    JwtRefreshToken = jwtRefreshToken,
                    JwtRefreshTokenExpiration = DateTime.UtcNow.AddDays(7)
                };

                await _userStorageService.StoreOrUpdateAsync(user);

                return Ok(new SpotifyUserClientResponse
                {
                    DisplayName = user.DisplayName,
                    SpotifyUserId = user.SpotifyUserId,
                    JwtToken = jwtToken,
                    JwtTokenExpiration = jwtTokenExpiration,
                    JwtRefreshToken = jwtRefreshToken,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Searches tracks on Spotify using the provided query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> SearchTracks([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query is required.");

           var (user, accessToken) = await _authenticateSpotifyUserService.GetCurrentUserWithValidTokenAsync();

            if (user == null || string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized("User not authenticated or access token is invalid.");
            }

            var results = await _spotifyApiService.SearchTracksAsync(query, accessToken, 10);

            return Ok(results); 
        }

        /// <summary>
        /// Toggle play/pause for the current track on the user's Spotify device.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("play")]
        public async Task<IActionResult> PlayTrack([FromBody] PlayTrackRequest request)
        {
            var (user, accessToken) = await _authenticateSpotifyUserService.GetCurrentUserWithValidTokenAsync();

            if(user == null || string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized("User not authenticated or access token is invalid.");
            }

            if (string.IsNullOrWhiteSpace(request.TrackId))
            {
                return BadRequest("TrackId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.DeviceId))
            {
                return BadRequest("DeviceId is required.");
            }

            var success = await _spotifyApiService.PlayTrackAsync(request.TrackId, request.DeviceId, accessToken);

            if (!success)
            {
                return StatusCode(500, "Failed to start playback on Spotify.");
            }

            return Ok(new { Message = "Playback started." });
        }

        [Authorize]
        [HttpPost("quiz")]
        public async Task<IActionResult> CreateQuiz([FromBody] QuizContent quizContent)
        {
            if (quizContent == null || quizContent.Questions == null || !quizContent.Questions.Any())
                return BadRequest("Quiz data is invalid.");

            // Get user ID from JWT
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var quiz = new Quiz
            {
                Title = quizContent.Title,
                SpotifyUserId = userId,
                Questions = quizContent.Questions.Select(q => new QuizQuestion
                {
                    Question = q.Question,
                    Answers = q.Answers,
                    CorrectAnswer = q.CorrectAnswer,
                    SpotifyTrack = q.SpotifyTrack, 
                }).ToList()
            };

            await _quizStorageService.StoreOrUpdateAsync(quiz);
            return Ok(new { Message = "Quiz created successfully." });
        }

        /// <summary>
        /// Retrieves the list of available devices for the authenticated user.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("devices")]
        public async Task<IActionResult> GetAvailableDevices()
        {
            var (user, accessToken) = await _authenticateSpotifyUserService.GetCurrentUserWithValidTokenAsync();

            if (user == null || string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized("User not authenticated or access token is invalid.");
            }

            var devices = await _spotifyApiService.GetAvailableDevicesAsync(accessToken);

            return Ok(devices);
        }

        [Authorize]
        [HttpGet("quizzes")]
        public async Task<IActionResult> GetUserQuizzesAsync()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var quizzes = await _quizStorageService.GetUserQuizzesAsync(userId);

            if (quizzes == null || !quizzes.Any())
            {
                return Ok(new List<QuizOption>());
            }

            return Ok(quizzes.Select(quiz => new QuizOption
            {
                Id = quiz.Id,
                Title = quiz.Title,
            })
            .ToList());
        }

        [HttpPost("refresh-jwt")]
        public async Task<IActionResult> RefreshJwtToken([FromBody] RefreshTokenRequest request)
        {
            var user = await _userStorageService.GetAsync(request.SpotifyUserId);

            if(user == null)
            {
                return NotFound("User not found.");
            }

            if (user.JwtRefreshToken != request.JwtRefreshToken)
            {
                return Unauthorized("Invalid refresh token.");
            }

            if( user.JwtRefreshTokenExpiration < DateTime.UtcNow)
            {
                return Unauthorized("Refresh token has expired.");
            }

            var newJwtToken = _encryptionHelper.GenerateJwtToken(user.SpotifyUserId, user.DisplayName);
            var newRefreshToken = _encryptionHelper.GenerateJwtRefreshToken();
            var newJwtTokenExpiration = _encryptionHelper.GetJwtTokenExpiration(newJwtToken);

            user.JwtRefreshToken = newRefreshToken;
            user.JwtRefreshTokenExpiration = DateTime.UtcNow.AddDays(7);
            await _userStorageService.StoreOrUpdateAsync(user);

            return Ok(new
            {
                JwtToken = newJwtToken,
                JwtTokenExpiration = newJwtTokenExpiration,
                JwtRefreshToken = newRefreshToken
            });
        }

        /// <summary>
        /// Pauses playback on the user's Spotify device.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("pause")]
        public async Task<IActionResult> PauseTrack([FromBody] PlayTrackRequest request)
        {
            var (user, accessToken) = await _authenticateSpotifyUserService.GetCurrentUserWithValidTokenAsync();

            if(user == null || string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized("User not authenticated or access token is invalid.");
            }

            if (string.IsNullOrWhiteSpace(request.DeviceId))
                return BadRequest("DeviceId is required.");

            var success = await _spotifyApiService.PauseTrackAsync(request.DeviceId, accessToken);

            if (!success)
            {
                return StatusCode(500, "Failed to pause playback on Spotify.");
            }

            return Ok(new { Message = "Playback paused." });
        }

       
    }
}
