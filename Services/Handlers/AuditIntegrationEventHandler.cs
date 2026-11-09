using Microsoft.Extensions.Logging;
using Studium_Audit.Data;
using Studium_Audit.Domain.Entities;
using Studium_Audit.Services.DTOs;

namespace Studium_Audit.Services.Handlers;

public class AuditIntegrationEventHandler(AuditDbContext db, ILogger<AuditIntegrationEventHandler> logger)
{
    public async Task HandleAsync(AuditIntegrationEvent message, CancellationToken ct = default)
    {
        var log = new AuditLog(
            serviceSource: message.ServiceSource,
            resourceId: message.ResourceId,
            actorId: message.ActorId,
            action: message.Action,
            metadataJson: message.MetadataJson,
            timestampUtc: message.TimestampUtc ?? DateTime.UtcNow);

        db.AuditLogs.Add(log);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Asynchronously persisted audit log {LogId} for {ServiceSource} - {Action} on {ResourceId}",
            log.Id, log.ServiceSource, log.Action, log.ResourceId);
    }
}
