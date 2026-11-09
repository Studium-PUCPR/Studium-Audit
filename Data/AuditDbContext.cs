using Microsoft.EntityFrameworkCore;
using Studium_Audit.Domain.Entities;
using Studium_Audit.Domain.Entities.Inbox;

namespace Studium_Audit.Data;

public class AuditDbContext(DbContextOptions<AuditDbContext> options) : DbContext(options)
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
