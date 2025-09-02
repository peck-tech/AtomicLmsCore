using System.Text.Json;
using AtomicLmsCore.Domain.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtomicLmsCore.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework configuration for the Tenant entity.
/// </summary>
[UsedImplicitly]
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenant");

        // Configure the Name property
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(255);

        // Configure the Slug property with unique constraint
        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => t.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Tenant_Slug");

        // Configure the DatabaseName property
        builder.Property(t => t.DatabaseName)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(t => t.DatabaseName)
            .IsUnique()
            .HasDatabaseName("IX_Tenant_DatabaseName");

        // Configure the IsActive property
        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Configure the Metadata property as JSON
        builder.Property(t => t.Metadata)
            .HasConversion(
                metadata => JsonSerializer.Serialize(metadata, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<Dictionary<string, string>>(json, (JsonSerializerOptions?)null) ??
                        new Dictionary<string, string>())
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        // Configure soft delete query filter
        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
