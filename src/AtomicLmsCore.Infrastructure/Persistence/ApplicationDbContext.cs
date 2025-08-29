using AtomicLmsCore.Domain;
using AtomicLmsCore.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace AtomicLmsCore.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly IIdGenerator _idGenerator;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        // Create a default ID generator for cases where DI isn't available
        _idGenerator = new Services.UlidIdGenerator();
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IIdGenerator idGenerator)
        : base(options)
    {
        _idGenerator = idGenerator;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure all entities inheriting from BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.IsSubclassOf(typeof(BaseEntity)))
            {
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
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
