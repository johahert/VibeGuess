using VibeGuess.Core.Interfaces;

namespace VibeGuess.Core.Entities;

/// <summary>
/// Base class for all entities with common functionality.
/// </summary>
public abstract class BaseEntity : IEntity, IAuditable
{
    /// <inheritdoc />
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <inheritdoc />
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updates the UpdatedAt timestamp.
    /// </summary>
    public virtual void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Base class for entities that can be soft deleted.
/// </summary>
public abstract class SoftDeleteEntity : BaseEntity, ISoftDelete
{
    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <inheritdoc />
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Soft deletes the entity.
    /// </summary>
    public virtual void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    /// <summary>
    /// Restores a soft-deleted entity.
    /// </summary>
    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        UpdateTimestamp();
    }
}

/// <summary>
/// Base class for user-owned entities.
/// </summary>
public abstract class UserOwnedEntity : BaseEntity, IUserOwned
{
    /// <inheritdoc />
    public Guid UserId { get; set; }
}

/// <summary>
/// Base class for expirable entities.
/// </summary>
public abstract class ExpirableEntity : BaseEntity, IExpirable
{
    /// <inheritdoc />
    public DateTime ExpiresAt { get; set; }

    /// <inheritdoc />
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Sets the expiration date relative to now.
    /// </summary>
    /// <param name="duration">How long from now the entity should expire.</param>
    public virtual void SetExpiration(TimeSpan duration)
    {
        ExpiresAt = DateTime.UtcNow.Add(duration);
        UpdateTimestamp();
    }

    /// <summary>
    /// Extends the expiration by the specified duration.
    /// </summary>
    /// <param name="duration">How much longer the entity should be valid.</param>
    public virtual void ExtendExpiration(TimeSpan duration)
    {
        ExpiresAt = ExpiresAt.Add(duration);
        UpdateTimestamp();
    }
}