using AtomicLmsCore.Domain;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Domain.Services;
using AtomicLmsCore.Infrastructure.Persistence.Configurations;
using AtomicLmsCore.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace AtomicLmsCore.Infrastructure.Persistence;

/// <summary>
///     Database context for tenant-specific data.
///     Each tenant has their own instance of this database.
/// </summary>
public class TenantDbContext : DbContext
{
    private readonly IIdGenerator _idGenerator;

    public TenantDbContext(DbContextOptions<TenantDbContext> options)
        : base(options)
    {
        // Create a default ID generator for cases where DI isn't available
        _idGenerator = new UlidIdGenerator();
    }

    public TenantDbContext(DbContextOptions<TenantDbContext> options, IIdGenerator idGenerator)
        : base(options)
    {
        _idGenerator = idGenerator;
    }

    /// <summary>
    ///     The Users table - tenant-specific entity.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    ///     The tenant identity validation table - ensures database belongs to correct tenant.
    /// </summary>
    public DbSet<TenantIdentity> TenantIdentity => Set<TenantIdentity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new UserTenantConfiguration());
        modelBuilder.ApplyConfiguration(new TenantIdentityConfiguration());

        // Configure all entities inheriting from BaseEntity that belong to this context
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Only configure entities that are explicitly mapped in this context
            // This prevents the Tenant entity from being configured here
            if (!entityType.ClrType.IsSubclassOf(typeof(BaseEntity)) ||
                entityType.ClrType == typeof(Tenant))
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
