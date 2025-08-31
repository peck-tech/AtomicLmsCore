using System.ComponentModel.DataAnnotations;

namespace AtomicLmsCore.WebApi.DTOs.Tenants;

/// <summary>
///     Data transfer object for updating an existing tenant.
/// </summary>
/// <param name="Name">The display name of the tenant.</param>
/// <param name="Slug">The unique slug/alias for the tenant.</param>
/// <param name="IsActive">Indicates whether the tenant is currently active.</param>
/// <param name="Metadata">Additional metadata for the tenant.</param>
public record UpdateTenantRequestDto(
    [property: Required]
    [property: StringLength(255)]
    string Name,
    [property: Required]
    [property: StringLength(100)]
    string Slug,
    [property: Required] bool IsActive,
    IDictionary<string, string>? Metadata = null);
