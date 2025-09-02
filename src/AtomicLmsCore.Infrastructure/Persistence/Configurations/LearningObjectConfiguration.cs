using System.Text.Json;
using AtomicLmsCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtomicLmsCore.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity configuration for LearningObject in the tenant-specific database.
/// </summary>
public class LearningObjectConfiguration : IEntityTypeConfiguration<LearningObject>
{
    public void Configure(EntityTypeBuilder<LearningObject> builder)
    {
        builder.ToTable("LearningObject");

        // Primary key is configured in the DbContext

        // Configure properties
        builder.Property(lo => lo.Name)
            .IsRequired()
            .HasMaxLength(500);

        // Configure metadata as JSON
        builder.Property(lo => lo.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<IDictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
            .HasColumnType("nvarchar(max)")
            .Metadata.SetValueComparer(
                new ValueComparer<IDictionary<string, string>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => new Dictionary<string, string>(c)));

        // Configure audit fields
        builder.Property(lo => lo.CreatedAt)
            .IsRequired();

        builder.Property(lo => lo.UpdatedAt)
            .IsRequired();

        builder.Property(lo => lo.CreatedBy)
            .HasMaxLength(255);

        builder.Property(lo => lo.UpdatedBy)
            .HasMaxLength(255);

        builder.Property(lo => lo.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes for performance
        builder.HasIndex(lo => lo.Name);

        // Add query filter for soft deletes
        builder.HasQueryFilter(lo => !lo.IsDeleted);
    }
}
