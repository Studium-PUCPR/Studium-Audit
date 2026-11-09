using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Studium_Audit.Domain.Entities.Inbox;

namespace Studium_Audit.Data.Configurations;

public class InboxMessageConfig : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.HandlerName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ReceivedAtUtc).IsRequired();
        builder.HasIndex(x => x.ReceivedAtUtc);
    }
}
