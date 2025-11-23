using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq.Expressions;
using VibeGuess.Core.Entities;
using VibeGuess.Infrastructure.Data.Configurations;

namespace VibeGuess.Infrastructure.Data;

/// <summary>
/// Main database context for the VibeGuess application.
/// </summary>
public class VibeGuessDbContext : DbContext
{
    public VibeGuessDbContext(DbContextOptions<VibeGuessDbContext> options) : base(options)
    {
    }

    // User Management
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserSettings> UserSettings { get; set; } = null!;
    public DbSet<SpotifyToken> SpotifyTokens { get; set; } = null!;

    // Quiz Domain
    public DbSet<Quiz> Quizzes { get; set; } = null!;
    public DbSet<QuizGenerationMetadata> QuizGenerationMetadata { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<AnswerOption> AnswerOptions { get; set; } = null!;
    public DbSet<Track> Tracks { get; set; } = null!;

    // Session Management  
    public DbSet<QuizSession> QuizSessions { get; set; } = null!;
    public DbSet<UserAnswer> UserAnswers { get; set; } = null!;
    
    // Live Session Analytics (Optional persistence)
    public DbSet<SessionSummary> SessionSummaries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new SpotifyTokenConfiguration());
        modelBuilder.ApplyConfiguration(new QuizConfiguration());
        modelBuilder.ApplyConfiguration(new QuizGenerationMetadataConfiguration());
        modelBuilder.ApplyConfiguration(new QuestionConfiguration());
        modelBuilder.ApplyConfiguration(new AnswerOptionConfiguration());
        modelBuilder.ApplyConfiguration(new TrackConfiguration());
        modelBuilder.ApplyConfiguration(new QuizSessionConfiguration());
        modelBuilder.ApplyConfiguration(new UserAnswerConfiguration());
        modelBuilder.ApplyConfiguration(new SessionSummaryConfiguration());

        // Configure base entity properties globally
        ConfigureBaseEntity(modelBuilder);

        // Configure audit timestamps
        ConfigureAuditTimestamps(modelBuilder);

        // Configure soft delete filter
        ConfigureSoftDeleteFilter(modelBuilder);
    }

    /// <summary>
    /// Configures common base entity properties.
    /// </summary>
    private static void ConfigureBaseEntity(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Configure primary keys as non-clustered for better performance
            if (entityType.ClrType.IsAssignableTo(typeof(BaseEntity)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasKey("Id");

                modelBuilder.Entity(entityType.ClrType)
                    .Property("Id")
                    .ValueGeneratedNever(); // We generate GUIDs in the entity constructors
            }
        }
    }

    /// <summary>
    /// Configures automatic timestamp updates.
    /// </summary>
    private static void ConfigureAuditTimestamps(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.IsAssignableTo(typeof(BaseEntity)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property("CreatedAt")
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                modelBuilder.Entity(entityType.ClrType)
                    .Property("UpdatedAt")
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAddOrUpdate();
            }
        }
    }

    /// <summary>
    /// Configures global query filters for soft delete.
    /// </summary>
    private static void ConfigureSoftDeleteFilter(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.IsAssignableTo(typeof(SoftDeleteEntity)))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, "IsDeleted");
                var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(false)), parameter);
                
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    /// <summary>
    /// Override SaveChanges to handle audit timestamps and soft delete.
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to handle audit timestamps and soft delete.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates audit fields before saving changes.
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }

            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}