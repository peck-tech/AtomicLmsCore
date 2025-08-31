using System.ComponentModel.DataAnnotations;

namespace AtomicLmsCore.WebApi.DTOs.Tenants;

/// <summary>
///     Data transfer object for tenant information.
/// </summary>
/// <param name="Id">The unique identifier of the tenant.</param>
/// <param name="Name">The display name of the tenant.</param>
/// <param name="Slug">The unique slug/alias for the tenant.</param>
/// <param name="IsActive">Indicates whether the tenant is currently active.</param>
/// <param name="CreatedAt">The date and time when the tenant was created.</param>
/// <param name="UpdatedAt">The date and time when the tenant was last updated.</param>
public record TenantDto(
    [property: Required] Guid Id,
    [property: Required] string Name,
    [property: Required] string Slug,
    [property: Required] bool IsActive,
    [property: Required] DateTime CreatedAt,
    [property: Required] DateTime UpdatedAt);
