using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeGuess.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tracks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpotifyTrackId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ArtistName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AllArtists = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AlbumName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
                    Popularity = table.Column<int>(type: "int", nullable: false),
                    IsExplicit = table.Column<bool>(type: "bit", nullable: false),
                    PreviewUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SpotifyUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AlbumImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AvailableMarkets = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Isrc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AudioFeatures = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpotifyUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    HasSpotifyPremium = table.Column<bool>(type: "bit", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "User"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UserPrompt = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Format = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Difficulty = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    QuestionCount = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false, defaultValue: "en"),
                    IncludesAudio = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Generated"),
                    PlayCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AverageScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quizzes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpotifyTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TokenType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Bearer"),
                    Scope = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotifyTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpotifyTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false, defaultValue: "en"),
                    EnableAudioPreview = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DefaultQuestionCount = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    DefaultDifficulty = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    RememberDeviceSelection = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastSelectedDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EnableHints = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ShuffleQuestions = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequiresAudio = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Points = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    TimeLimitSeconds = table.Column<int>(type: "int", nullable: true),
                    HintText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrackId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Questions_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "QuizGenerationMetadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcessingTimeMs = table.Column<int>(type: "int", nullable: false),
                    AiModel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AiModelVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TracksFound = table.Column<int>(type: "int", nullable: false),
                    TracksValidated = table.Column<int>(type: "int", nullable: false),
                    TokensConsumed = table.Column<int>(type: "int", nullable: false),
                    EstimatedCostCents = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    Warnings = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RawPrompt = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RawResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizGenerationMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizGenerationMetadata_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CurrentQuestionIndex = table.Column<int>(type: "int", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    CurrentScore = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxPossibleScore = table.Column<int>(type: "int", nullable: false),
                    ShuffleQuestions = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EnableHints = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalTimeSeconds = table.Column<int>(type: "int", nullable: true),
                    SessionConfig = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizSessions_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnswerOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionLabel = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnswerOptions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelectedAnswerOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FreeTextAnswer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PointsEarned = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TimeToAnswerSeconds = table.Column<int>(type: "int", nullable: true),
                    UsedHint = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PlaybackCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuestionStartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnswerMetadata = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAnswers_AnswerOptions_SelectedAnswerOptionId",
                        column: x => x.SelectedAnswerOptionId,
                        principalTable: "AnswerOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAnswers_QuizSessions_QuizSessionId",
                        column: x => x.QuizSessionId,
                        principalTable: "QuizSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerOptions_QuestionId",
                table: "AnswerOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerOptions_QuestionId_IsCorrect",
                table: "AnswerOptions",
                columns: new[] { "QuestionId", "IsCorrect" });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerOptions_QuestionId_OrderIndex",
                table: "AnswerOptions",
                columns: new[] { "QuestionId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuizId",
                table: "Questions",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuizId_OrderIndex",
                table: "Questions",
                columns: new[] { "QuizId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_TrackId",
                table: "Questions",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizGenerationMetadata_AiModel",
                table: "QuizGenerationMetadata",
                column: "AiModel");

            migrationBuilder.CreateIndex(
                name: "IX_QuizGenerationMetadata_CreatedAt",
                table: "QuizGenerationMetadata",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QuizGenerationMetadata_QuizId",
                table: "QuizGenerationMetadata",
                column: "QuizId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessions_QuizId",
                table: "QuizSessions",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessions_StartedAt",
                table: "QuizSessions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessions_Status_ExpiresAt",
                table: "QuizSessions",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessions_UserId",
                table: "QuizSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessions_UserId_Status",
                table: "QuizSessions",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_CreatedAt",
                table: "Quizzes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_IsPublic_Status",
                table: "Quizzes",
                columns: new[] { "IsPublic", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_Status_ExpiresAt",
                table: "Quizzes",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_UserId",
                table: "Quizzes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SpotifyTokens_ExpiresAt",
                table: "SpotifyTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_SpotifyTokens_UserId",
                table: "SpotifyTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SpotifyTokens_UserId_IsActive",
                table: "SpotifyTokens",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_AlbumName",
                table: "Tracks",
                column: "AlbumName");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_ArtistName",
                table: "Tracks",
                column: "ArtistName");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_Popularity",
                table: "Tracks",
                column: "Popularity");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_ReleaseDate",
                table: "Tracks",
                column: "ReleaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_SpotifyTrackId",
                table: "Tracks",
                column: "SpotifyTrackId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_AnsweredAt",
                table: "UserAnswers",
                column: "AnsweredAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_IsCorrect_PointsEarned",
                table: "UserAnswers",
                columns: new[] { "IsCorrect", "PointsEarned" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_QuestionId",
                table: "UserAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_QuizSessionId",
                table: "UserAnswers",
                column: "QuizSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_QuizSessionId_QuestionId",
                table: "UserAnswers",
                columns: new[] { "QuizSessionId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_SelectedAnswerOptionId",
                table: "UserAnswers",
                column: "SelectedAnswerOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role_IsActive",
                table: "Users",
                columns: new[] { "Role", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_SpotifyUserId",
                table: "Users",
                column: "SpotifyUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizGenerationMetadata");

            migrationBuilder.DropTable(
                name: "SpotifyTokens");

            migrationBuilder.DropTable(
                name: "UserAnswers");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "AnswerOptions");

            migrationBuilder.DropTable(
                name: "QuizSessions");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.DropTable(
                name: "Tracks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
