namespace AtomicLmsCore.Domain.Services;

/// <summary>
///     Service for generating sequential, opaque identifiers for public API exposure.
/// </summary>
public interface IIdGenerator
{
    /// <summary>
    ///     Generates a new sequential GUID that can be converted from/to ULID.
    ///     Sequential nature reduces database index fragmentation.
    /// </summary>
    /// <returns>A sequential GUID suitable for public API exposure.</returns>
    Guid NewId();
}
