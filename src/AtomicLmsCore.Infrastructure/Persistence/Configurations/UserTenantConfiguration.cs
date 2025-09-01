using AtomicLmsCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtomicLmsCore.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity configuration for User in the tenant-specific database.
///     No tenant relationship since each database is tenant-specific.
/// </summary>
public class UserTenantConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");

        // Primary key is configured in the DbContext

        // Configure properties
        builder.Property(u => u.ExternalUserId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Configure metadata as JSON
        builder.Property(u => u.Metadata)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<IDictionary<string, string>>(
                    v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
            .HasColumnType("nvarchar(max)")
            .Metadata.SetValueComparer(new ValueComparer<IDictionary<string, string>>(
                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => new Dictionary<string, string>(c)));

        // Indexes for performance - no need for tenant filtering
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.ExternalUserId)
            .IsUnique();

        // Ignore the Tenant navigation property since we're in a tenant-specific database
        builder.Ignore(u => u.Tenant);
        builder.Ignore(u => u.TenantInternalId);

        // Configure audit fields
        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .IsRequired();

        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Add query filter for soft deletes
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
