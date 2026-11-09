using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Studium_Audit.Data;
using Studium_Audit.Domain.Entities;
using Studium_Audit.Domain.Entities.Inbox;
using Studium_Audit.Services.DTOs;

namespace Studium_Audit.Services.Handlers;

public class AuditIntegrationEventHandler(AuditDbContext db, ILogger<AuditIntegrationEventHandler> logger)
{
    private static DateTime _lastCleanupUtc = DateTime.MinValue;
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(6);
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(7);

    public async Task HandleAsync(AuditIntegrationEvent message, CancellationToken ct = default)
    {
        var eventId = message.EventId ?? Guid.NewGuid();

        var alreadyProcessed = await db.InboxMessages.AnyAsync(x => x.Id == eventId, ct);
        if (alreadyProcessed)
        {
            logger.LogInformation("Inbox duplicate detected for AuditIntegrationEvent {EventId}. Skipping processing.", eventId);
            return;
        }

        var log = new AuditLog(
            serviceSource: message.ServiceSource,
            resourceId: message.ResourceId,
            actorId: message.ActorId,
            action: message.Action,
            metadataJson: message.MetadataJson,
            timestampUtc: message.TimestampUtc ?? DateTime.UtcNow);

        db.AuditLogs.Add(log);
        db.InboxMessages.Add(new InboxMessage(eventId, nameof(AuditIntegrationEventHandler), DateTime.UtcNow));
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Asynchronously persisted audit log {LogId} for {ServiceSource} - {Action} on {ResourceId}",
            log.Id, log.ServiceSource, log.Action, log.ResourceId);

        var nowUtc = DateTime.UtcNow;
        if (nowUtc - _lastCleanupUtc > CleanupInterval)
        {
            _lastCleanupUtc = nowUtc;
            var threshold = nowUtc - RetentionPeriod;
            var deletedCount = await db.InboxMessages
                .Where(m => m.ReceivedAtUtc < threshold)
                .ExecuteDeleteAsync(ct);
            if (deletedCount > 0)
            {
                logger.LogInformation("Lazy pruned {Count} expired inbox messages older than {Threshold}", deletedCount, threshold);
            }
        }
    }
}
