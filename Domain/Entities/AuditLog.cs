namespace Studium_Audit.Domain.Entities;

/// <summary>
/// Immutable audit log entry capturing actions across any Studium microservice.
/// </summary>
public class AuditLog
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Microservice that emitted the audit entry (e.g. "Users", "Booking", "Catalog").
    /// </summary>
    public string ServiceSource { get; init; } = string.Empty;

    /// <summary>
    /// Generic identifier of the affected entity (e.g. UserId, BookingId, RoomId).
    /// </summary>
    public string ResourceId { get; init; } = string.Empty;

    /// <summary>
    /// Actor who performed the action (User UUID, Admin UUID, or "SYSTEM").
    /// </summary>
    public string ActorId { get; init; } = string.Empty;

    /// <summary>
    /// Action performed (e.g., "UserBlocked", "NoShowRecorded", "MaintenanceScheduled", "ProfileUpdated").
    /// </summary>
    public string Action { get; init; } = string.Empty;

    /// <summary>
    /// Flexible structured context (e.g. JSON metadata, reason, previous/new values).
    /// </summary>
    public string? MetadataJson { get; init; }

    /// <summary>
    /// UTC timestamp when the event occurred.
    /// </summary>
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;

    public AuditLog() { }

    public AuditLog(
        string serviceSource,
        string resourceId,
        string actorId,
        string action,
        string? metadataJson = null,
        DateTime? timestampUtc = null,
        Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        ServiceSource = serviceSource;
        ResourceId = resourceId;
        ActorId = actorId;
        Action = action;
        MetadataJson = metadataJson;
        TimestampUtc = timestampUtc ?? DateTime.UtcNow;
    }
}
