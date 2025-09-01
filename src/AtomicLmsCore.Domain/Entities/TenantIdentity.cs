namespace AtomicLmsCore.Domain.Entities;

/// <summary>
///     Represents the identity validation record stored in each tenant database.
///     This ensures that the database actually belongs to the correct tenant.
/// </summary>
public class TenantIdentity
{
    /// <summary>
    ///     The unique identifier of the tenant this database belongs to.
    ///     Must match the tenant ID from the Solutions database.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    ///     The name of this database as configured in the tenant entity.
    ///     Used for cross-validation.
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    ///     When this tenant database was created and this identity record was established.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Validation hash to prevent tampering.
    ///     Hash of TenantId + DatabaseName + CreatedAt + ValidationSecret.
    /// </summary>
    public string ValidationHash { get; set; } = string.Empty;

    /// <summary>
    ///     Optional metadata about the database creation process.
    /// </summary>
    public string CreationMetadata { get; set; } = string.Empty;
}
