using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.Ordering.Domain.Orders;

namespace Pos.Ordering.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id).ValueGeneratedNever();

        builder.Property(order => order.OrderNumber).HasMaxLength(Order.OrderNumberMaxLength).IsRequired();
        builder.HasIndex(order => order.OrderNumber).IsUnique();
        builder.Property(order => order.CashierId).HasMaxLength(Order.CashierIdMaxLength).IsRequired();
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(order => new { order.Status, order.CreatedOnUtc });
        builder.Property(order => order.CancellationReason).HasMaxLength(Order.ReasonMaxLength);

        builder.ComplexProperty(order => order.Total, total =>
        {
            total.Property(money => money.Amount).HasColumnName("total_amount").HasPrecision(18, 2);
            total.Property(money => money.Currency).HasColumnName("total_currency").HasMaxLength(3);
        });

        builder.HasMany(order => order.Lines)
            .WithOne()
            .HasForeignKey(line => line.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(order => order.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(order => order.DomainEvents);
    }
}
