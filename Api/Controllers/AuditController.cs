using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studium_Audit.Data;
using Studium_Audit.Domain.Entities;
using Studium_Audit.Services.DTOs;

namespace Studium_Audit.Api.Controllers;

[ApiController]
[Route("api/audit")]
public class AuditController(AuditDbContext db, ILogger<AuditController> logger) : ControllerBase
{
    [HttpPost("logs")]
    public async Task<ActionResult<AuditLogDto>> CreateAuditLog(
        [FromBody] CreateAuditLogRequest request,
        CancellationToken ct)
    {
        var log = new AuditLog(
            serviceSource: request.ServiceSource,
            resourceId: request.ResourceId,
            actorId: request.ActorId,
            action: request.Action,
            metadataJson: request.MetadataJson);

        db.AuditLogs.Add(log);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Audit entry {LogId} recorded for {ServiceSource}:{Action} on resource {ResourceId}",
            log.Id, log.ServiceSource, log.Action, log.ResourceId);

        var dto = new AuditLogDto(
            log.Id,
            log.ServiceSource,
            log.ResourceId,
            log.ActorId,
            log.Action,
            log.MetadataJson,
            log.TimestampUtc);

        return CreatedAtAction(nameof(GetLogsByResource), new { resourceId = log.ResourceId }, dto);
    }

    [HttpGet("logs")]
    public async Task<ActionResult<PagedAuditLogsResponse>> GetAuditLogs(
        [FromQuery] string? resourceId,
        [FromQuery] string? serviceSource,
        [FromQuery] string? actorId,
        [FromQuery] string? action,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(resourceId))
            query = query.Where(l => l.ResourceId == resourceId);

        if (!string.IsNullOrWhiteSpace(serviceSource))
            query = query.Where(l => l.ServiceSource == serviceSource);

        if (!string.IsNullOrWhiteSpace(actorId))
            query = query.Where(l => l.ActorId == actorId);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(l => l.Action == action);

        if (fromUtc.HasValue)
            query = query.Where(l => l.TimestampUtc >= fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(l => l.TimestampUtc <= toUtc.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.TimestampUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AuditLogDto(
                l.Id,
                l.ServiceSource,
                l.ResourceId,
                l.ActorId,
                l.Action,
                l.MetadataJson,
                l.TimestampUtc))
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Ok(new PagedAuditLogsResponse(items, totalCount, page, pageSize, totalPages));
    }

    [HttpGet("resources/{resourceId}")]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> GetLogsByResource(
        string resourceId,
        CancellationToken ct = default)
    {
        var items = await db.AuditLogs
            .AsNoTracking()
            .Where(l => l.ResourceId == resourceId)
            .OrderByDescending(l => l.TimestampUtc)
            .Take(100)
            .Select(l => new AuditLogDto(
                l.Id,
                l.ServiceSource,
                l.ResourceId,
                l.ActorId,
                l.Action,
                l.MetadataJson,
                l.TimestampUtc))
            .ToListAsync(ct);

        return Ok(items);
    }
}
