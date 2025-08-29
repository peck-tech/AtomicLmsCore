using AtomicLmsCore.Domain.Services;

namespace AtomicLmsCore.Infrastructure.Services;

/// <summary>
/// ULID-based sequential ID generator that converts to GUID for API consistency.
/// ULIDs are lexicographically sortable and contain timestamp information.
/// </summary>
public class UlidIdGenerator : IIdGenerator
{
    public Guid NewId()
    {
        // Generate a new ULID and convert to Guid
        var ulid = Ulid.NewUlid();
        return ulid.ToGuid();
    }
}