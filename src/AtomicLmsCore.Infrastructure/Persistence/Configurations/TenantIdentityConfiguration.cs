using AtomicLmsCore.Domain.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtomicLmsCore.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework configuration for the TenantIdentity entity.
///     This entity exists only in tenant-specific databases for validation.
/// </summary>
[UsedImplicitly]
public class TenantIdentityConfiguration : IEntityTypeConfiguration<TenantIdentity>
{
    public void Configure(EntityTypeBuilder<TenantIdentity> builder)
    {
        builder.ToTable("__tenant_identity");

        // Use TenantId as primary key since there's only one record per tenant database
        builder.HasKey(ti => ti.TenantId);

        builder.Property(ti => ti.TenantId)
            .IsRequired();

        builder.Property(ti => ti.DatabaseName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ti => ti.CreatedAt)
            .IsRequired();

        builder.Property(ti => ti.ValidationHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(ti => ti.CreationMetadata)
            .HasMaxLength(1000)
            .HasDefaultValue(string.Empty);

        // Add index on database name for quick validation lookups
        builder.HasIndex(ti => ti.DatabaseName);

        // Add index on validation hash for integrity checks
        builder.HasIndex(ti => ti.ValidationHash);
    }
}
