using Microsoft.EntityFrameworkCore;
using Pos.BuildingBlocks.Infrastructure.Inbox;
using Pos.BuildingBlocks.Infrastructure.Outbox;

namespace Pos.BuildingBlocks.Infrastructure.Persistence;

public static class MessagingModelBuilderExtensions
{
    /// <summary>Maps the <c>outbox_messages</c> and <c>inbox_messages</c> tables.</summary>
    public static ModelBuilder ApplyMessagingConfiguration(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("outbox_messages");
            builder.HasKey(message => message.Id);
            builder.Property(message => message.Type).HasMaxLength(256).IsRequired();
            builder.Property(message => message.Content).IsRequired();
            builder.Property(message => message.Error).HasMaxLength(2000);
            builder.HasIndex(message => new { message.ProcessedOnUtc, message.OccurredOnUtc });
        });

        modelBuilder.Entity<InboxMessage>(builder =>
        {
            builder.ToTable("inbox_messages");
            builder.HasKey(message => new { message.MessageId, message.Consumer });
            builder.Property(message => message.MessageId).HasMaxLength(64);
            builder.Property(message => message.Consumer).HasMaxLength(256);
        });

        return modelBuilder;
    }
}
