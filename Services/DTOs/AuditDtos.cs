namespace Studium_Audit.Services.DTOs;

public record CreateAuditLogRequest(
    string ServiceSource,
    string ResourceId,
    string ActorId,
    string Action,
    string? MetadataJson = null);

public record AuditLogDto(
    Guid Id,
    string ServiceSource,
    string ResourceId,
    string ActorId,
    string Action,
    string? MetadataJson,
    DateTime TimestampUtc);

public record PagedAuditLogsResponse(
    IReadOnlyList<AuditLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
