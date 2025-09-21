using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeGuess.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadePaths2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_QuizSessions_QuizSessionId",
                table: "UserAnswers");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_QuizSessions_QuizSessionId",
                table: "UserAnswers",
                column: "QuizSessionId",
                principalTable: "QuizSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_QuizSessions_QuizSessionId",
                table: "UserAnswers");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_QuizSessions_QuizSessionId",
                table: "UserAnswers",
                column: "QuizSessionId",
                principalTable: "QuizSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
