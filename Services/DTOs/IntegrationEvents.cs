namespace Studium_Audit.Services.DTOs;

public record AuditIntegrationEvent(
    string ServiceSource,
    string ResourceId,
    string ActorId,
    string Action,
    string? MetadataJson = null,
    DateTime? TimestampUtc = null);
