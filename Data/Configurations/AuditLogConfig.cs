using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Studium_Audit.Domain.Entities;

namespace Studium_Audit.Data.Configurations;

public class AuditLogConfig : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> entity)
    {
        entity.ToTable("AuditLogs");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedNever();

        entity.Property(e => e.ServiceSource)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(e => e.ResourceId)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.ActorId)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.Action)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.MetadataJson)
            .HasColumnType("text");

        entity.Property(e => e.TimestampUtc)
            .IsRequired();

        entity.HasIndex(e => e.ResourceId);
        entity.HasIndex(e => e.ServiceSource);
        entity.HasIndex(e => e.ActorId);
        entity.HasIndex(e => e.Action);
        entity.HasIndex(e => e.TimestampUtc);
    }
}
