namespace AtomicLmsCore.Domain;

public abstract class BaseEntity
{
    /// <summary>
    ///     Internal database primary key for performance and relationships.
    ///     Not exposed in public APIs.
    /// </summary>
    public int InternalId { get; set; }

    /// <summary>
    ///     Public-facing opaque identifier for API exposure.
    ///     Sequential ULID-based GUID to prevent enumeration attacks and reduce index fragmentation.
    ///     Generated automatically when entity is added to context.
    /// </summary>
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public string UpdatedBy { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }
}
