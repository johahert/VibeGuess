namespace VibeGuess.Core.Interfaces;

/// <summary>
/// Base interface for all entities with a unique identifier.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    Guid Id { get; set; }
}

/// <summary>
/// Interface for entities that track creation timestamp.
/// </summary>
public interface ICreatedAt
{
    /// <summary>
    /// When the entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }
}

/// <summary>
/// Interface for entities that track update timestamp.
/// </summary>
public interface IUpdatedAt
{
    /// <summary>
    /// When the entity was last updated.
    /// </summary>
    DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Interface for entities that support soft deletion.
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// Whether the entity has been soft deleted.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// When the entity was deleted (if applicable).
    /// </summary>
    DateTime? DeletedAt { get; set; }
}

/// <summary>
/// Interface for auditable entities with full timestamp tracking.
/// </summary>
public interface IAuditable : ICreatedAt, IUpdatedAt
{
}

/// <summary>
/// Interface for entities that can be owned by a user.
/// </summary>
public interface IUserOwned
{
    /// <summary>
    /// ID of the user who owns this entity.
    /// </summary>
    Guid UserId { get; set; }
}

/// <summary>
/// Interface for entities that can expire.
/// </summary>
public interface IExpirable
{
    /// <summary>
    /// When the entity expires.
    /// </summary>
    DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether the entity has expired.
    /// </summary>
    bool IsExpired { get; }
}