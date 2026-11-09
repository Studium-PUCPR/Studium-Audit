using Studium_Audit.Domain.Entities;
using Xunit;

namespace Studium_Audit.Tests.Domain;

public class AuditLogTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var log = new AuditLog(
            serviceSource: "Users",
            resourceId: "user-123",
            actorId: "admin-456",
            action: "UserBlocked",
            metadataJson: "{\"reason\":\"Infraction\"}",
            timestampUtc: now,
            id: id);

        Assert.Equal(id, log.Id);
        Assert.Equal("Users", log.ServiceSource);
        Assert.Equal("user-123", log.ResourceId);
        Assert.Equal("admin-456", log.ActorId);
        Assert.Equal("UserBlocked", log.Action);
        Assert.Equal("{\"reason\":\"Infraction\"}", log.MetadataJson);
        Assert.Equal(now, log.TimestampUtc);
    }

    [Fact]
    public void DefaultConstructor_ShouldGenerateNewGuidAndUtcTimestamp()
    {
        var log = new AuditLog();

        Assert.NotEqual(Guid.Empty, log.Id);
        Assert.True(log.TimestampUtc <= DateTime.UtcNow);
    }
}
