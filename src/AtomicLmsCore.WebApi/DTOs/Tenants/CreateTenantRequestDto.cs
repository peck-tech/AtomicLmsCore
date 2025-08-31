using System.ComponentModel.DataAnnotations;

namespace AtomicLmsCore.WebApi.DTOs.Tenants;

/// <summary>
///     Data transfer object for creating a new tenant.
/// </summary>
/// <param name="Name">The display name of the tenant.</param>
/// <param name="Slug">The unique slug/alias for the tenant.</param>
/// <param name="IsActive">Indicates whether the tenant is currently active. Defaults to true.</param>
/// <param name="Metadata">Additional metadata for the tenant.</param>
public record CreateTenantRequestDto(
    [property: Required]
    [property: StringLength(255)]
    string Name,
    [property: Required]
    [property: StringLength(100)]
    string Slug,
    bool IsActive = true,
    IDictionary<string, string>? Metadata = null);
