using AtomicLmsCore.Domain;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Domain.Services;
using AtomicLmsCore.Infrastructure.Persistence.Configurations;
using AtomicLmsCore.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace AtomicLmsCore.Infrastructure.Persistence;

/// <summary>
///     Database context for the Solutions feature bucket.
///     Contains cross-tenant entities like Tenant configuration.
/// </summary>
public class SolutionsDbContext : DbContext
{
    private readonly IIdGenerator _idGenerator;

    public SolutionsDbContext(DbContextOptions<SolutionsDbContext> options)
        : base(options)
    {
        // Create a default ID generator for cases where DI isn't available
        _idGenerator = new UlidIdGenerator();
    }

    public SolutionsDbContext(DbContextOptions<SolutionsDbContext> options, IIdGenerator idGenerator)
        : base(options)
    {
        _idGenerator = idGenerator;
    }

    /// <summary>
    ///     The Tenants table - cross-tenant entity.
    /// </summary>
    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new TenantConfiguration());

        // Configure all entities inheriting from BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!entityType.ClrType.IsSubclassOf(typeof(BaseEntity)))
            {
                continue;
            }

            // Set InternalId as the primary key
            modelBuilder.Entity(entityType.ClrType)
                .HasKey(nameof(BaseEntity.InternalId));

            // Create a unique index on the public Id for performance
            modelBuilder.Entity(entityType.ClrType)
                .HasIndex(nameof(BaseEntity.Id))
                .IsUnique();

            // Configure InternalId as identity column
            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(BaseEntity.InternalId))
                .ValueGeneratedOnAdd();
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(BaseEntity.CreatedAt)).CurrentValue = DateTime.UtcNow;
                    entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = DateTime.UtcNow;
                    // Generate sequential ULID-based GUID if not already set
                    if (entry.Entity.Id == Guid.Empty)
                    {
                        entry.Entity.Id = _idGenerator.NewId();
                    }

                    break;
                case EntityState.Modified:
                    entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = DateTime.UtcNow;
                    break;
                case EntityState.Detached:
                case EntityState.Unchanged:
                case EntityState.Deleted:
                default:
                    continue;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
