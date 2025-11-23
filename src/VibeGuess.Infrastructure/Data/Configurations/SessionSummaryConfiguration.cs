using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeGuess.Core.Entities;

namespace VibeGuess.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for SessionSummary.
/// </summary>
public class SessionSummaryConfiguration : IEntityTypeConfiguration<SessionSummary>
{
    public void Configure(EntityTypeBuilder<SessionSummary> builder)
    {
        builder.ToTable("SessionSummaries");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SessionId)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Unique identifier from the live session (Redis)");

        builder.Property(s => s.JoinCode)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Join code used during the live session");

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Session title");

        builder.Property(s => s.QuizId)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Reference to the quiz that was played");

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasComment("When the live session was created");

        builder.Property(s => s.StartedAt)
            .HasComment("When the game started");

        builder.Property(s => s.EndedAt)
            .HasComment("When the game ended");

        builder.Property(s => s.ParticipantCount)
            .IsRequired()
            .HasComment("Total number of participants");

        builder.Property(s => s.TotalQuestions)
            .IsRequired()
            .HasComment("Number of questions in the quiz");

        builder.Property(s => s.TotalAnswers)
            .IsRequired()
            .HasComment("Total answers submitted across all participants");

        builder.Property(s => s.AverageScore)
            .HasPrecision(10, 2)
            .HasComment("Average score across all participants");

        builder.Property(s => s.AverageAccuracy)
            .HasPrecision(5, 2)
            .HasComment("Average accuracy percentage across all participants");

        builder.Property(s => s.AverageResponseTime)
            .HasComment("Average response time across all answers");

        builder.Property(s => s.LeaderboardJson)
            .HasComment("JSON-serialized leaderboard data (top 20 participants)");

        builder.Property(s => s.QuestionStatsJson)
            .HasComment("JSON-serialized question-level statistics");

        builder.Property(s => s.ParticipantDetailsJson)
            .HasComment("JSON-serialized detailed participant data");

        // Indexes for common queries
        builder.HasIndex(s => s.SessionId)
            .IsUnique()
            .HasDatabaseName("IX_SessionSummaries_SessionId");

        builder.HasIndex(s => s.CreatedAt)
            .HasDatabaseName("IX_SessionSummaries_CreatedAt");

        builder.HasIndex(s => s.AverageScore)
            .HasDatabaseName("IX_SessionSummaries_AverageScore");

        builder.HasIndex(s => new { s.CreatedAt, s.EndedAt })
            .HasDatabaseName("IX_SessionSummaries_DateRange");
    }
}