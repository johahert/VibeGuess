using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeGuess.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveSessionSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionSummaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Unique identifier from the live session (Redis)"),
                    JoinCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Join code used during the live session"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Session title"),
                    QuizId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Reference to the quiz that was played"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "When the live session was created"),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "When the game started"),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "When the game ended"),
                    ParticipantCount = table.Column<int>(type: "int", nullable: false, comment: "Total number of participants"),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false, comment: "Number of questions in the quiz"),
                    TotalAnswers = table.Column<int>(type: "int", nullable: false, comment: "Total answers submitted across all participants"),
                    AverageScore = table.Column<double>(type: "float(10)", precision: 10, scale: 2, nullable: false, comment: "Average score across all participants"),
                    AverageAccuracy = table.Column<double>(type: "float(5)", precision: 5, scale: 2, nullable: false, comment: "Average accuracy percentage across all participants"),
                    AverageResponseTime = table.Column<TimeSpan>(type: "time", nullable: false, comment: "Average response time across all answers"),
                    LeaderboardJson = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: true, comment: "JSON-serialized leaderboard data (top 20 participants)"),
                    QuestionStatsJson = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: true, comment: "JSON-serialized question-level statistics"),
                    ParticipantDetailsJson = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: true, comment: "JSON-serialized detailed participant data"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionSummaries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionSummaries_AverageScore",
                table: "SessionSummaries",
                column: "AverageScore");

            migrationBuilder.CreateIndex(
                name: "IX_SessionSummaries_CreatedAt",
                table: "SessionSummaries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SessionSummaries_DateRange",
                table: "SessionSummaries",
                columns: new[] { "CreatedAt", "EndedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SessionSummaries_SessionId",
                table: "SessionSummaries",
                column: "SessionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionSummaries");
        }
    }
}
