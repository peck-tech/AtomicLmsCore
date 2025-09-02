using System.Text.Json;
using AtomicLmsCore.Domain.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtomicLmsCore.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework configuration for the User entity.
/// </summary>
[UsedImplicitly]
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");

        builder.HasKey(u => u.InternalId);

        builder.Property(u => u.InternalId)
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Id)
            .IsRequired();

        builder.HasIndex(u => u.Id)
            .IsUnique();

        builder.Property(u => u.ExternalUserId)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.ExternalUserId)
            .IsUnique();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => new
        {
            u.Email,
            u.TenantInternalId,
        })
            .HasFilter("[IsDeleted] = 0");

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
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.UpdatedBy)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasQueryFilter(u => !u.IsDeleted);

        builder.Property(u => u.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
            .HasColumnType("nvarchar(max)");

        builder.HasOne(u => u.Tenant)
            .WithMany()
            .HasForeignKey(u => u.TenantInternalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.TenantInternalId);
    }
}
