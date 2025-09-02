namespace AtomicLmsCore.Domain.Entities;

/// <summary>
///     Represents a learning object in the LMS system.
/// </summary>
public class LearningObject : BaseEntity
{
    /// <summary>
    ///     The name of the learning object.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Additional metadata for the learning object.
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
