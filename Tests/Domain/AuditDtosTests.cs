using Studium_Audit.Services.DTOs;
using Xunit;

namespace Studium_Audit.Tests.Domain;

public class AuditDtosTests
{
    [Fact]
    public void CreateAuditLogRequest_ShouldMapCorrectly()
    {
        var req = new CreateAuditLogRequest("Booking", "b-123", "student-1", "BookingCreated", "{\"room\":\"A101\"}");

        Assert.Equal("Booking", req.ServiceSource);
        Assert.Equal("b-123", req.ResourceId);
        Assert.Equal("student-1", req.ActorId);
        Assert.Equal("BookingCreated", req.Action);
        Assert.Equal("{\"room\":\"A101\"}", req.MetadataJson);
    }

    [Fact]
    public void PagedAuditLogsResponse_ShouldComputePaginationCorrectly()
    {
        var dtos = new List<AuditLogDto>
        {
            new(Guid.NewGuid(), "Users", "u-1", "admin", "UserBlocked", null, DateTime.UtcNow)
        };

        var response = new PagedAuditLogsResponse(dtos, 10, 1, 5, 2);

        Assert.Single(response.Items);
        Assert.Equal(10, response.TotalCount);
        Assert.Equal(1, response.Page);
        Assert.Equal(5, response.PageSize);
        Assert.Equal(2, response.TotalPages);
    }
}
