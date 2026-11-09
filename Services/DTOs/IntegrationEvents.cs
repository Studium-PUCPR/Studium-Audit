using Wolverine.Attributes;

namespace Studium_Audit.Services.DTOs;

/// <summary>
/// Inbound message payload consumed from studium-audit-events-queue for compliance logging.
/// </summary>
[MessageIdentity("studium.audit.integration-event")]
public record AuditIntegrationEvent(
    string ServiceSource,
    string ResourceId,
    string ActorId,
    string Action,
    string? MetadataJson = null,
    DateTime? TimestampUtc = null,
    Guid? EventId = null);
