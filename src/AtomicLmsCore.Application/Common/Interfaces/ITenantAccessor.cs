namespace AtomicLmsCore.Application.Common.Interfaces;

/// <summary>
///     Provides access to the current tenant context from HTTP headers.
/// </summary>
public interface ITenantAccessor
{
    /// <summary>
    ///     Gets the current tenant ID from the HTTP context header.
    /// </summary>
    /// <returns>The current tenant's public identifier, or null if not found.</returns>
    Guid? GetCurrentTenantId();

    /// <summary>
    ///     Gets the current tenant ID from the HTTP context header, throwing an exception if not found.
    /// </summary>
    /// <returns>The current tenant's public identifier.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no tenant ID is found in the context.</exception>
    Guid GetRequiredCurrentTenantId();
}
