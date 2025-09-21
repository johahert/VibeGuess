using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeGuess.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizSessions_Users_UserId",
                table: "QuizSessions");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizSessions_Users_UserId",
                table: "QuizSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizSessions_Users_UserId",
                table: "QuizSessions");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizSessions_Users_UserId",
                table: "QuizSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
