using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.Inventory.Domain.StockItems;

namespace Pos.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();

        builder.HasIndex(item => item.ProductId).IsUnique();
        builder.Property(item => item.Sku).HasMaxLength(StockItem.SkuMaxLength).IsRequired();
        builder.Property(item => item.ProductName).HasMaxLength(StockItem.NameMaxLength).IsRequired();
        builder.Property(item => item.Version).IsConcurrencyToken();

        builder.HasMany(item => item.Reservations)
            .WithOne()
            .HasForeignKey(reservation => reservation.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(item => item.Reservations).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(item => item.QuantityAvailable);
        builder.Ignore(item => item.DomainEvents);
    }
}
