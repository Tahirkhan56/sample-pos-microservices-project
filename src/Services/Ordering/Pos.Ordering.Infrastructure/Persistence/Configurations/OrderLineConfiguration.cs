using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.Ordering.Domain.Orders;

namespace Pos.Ordering.Infrastructure.Persistence.Configurations;

internal sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("order_lines");
        builder.HasKey(line => line.Id);
        builder.Property(line => line.Id).ValueGeneratedNever();
        builder.Property(line => line.ProductName).HasMaxLength(OrderLine.ProductNameMaxLength).IsRequired();

        builder.ComplexProperty(line => line.UnitPrice, price =>
        {
            price.Property(money => money.Amount).HasColumnName("unit_price_amount").HasPrecision(18, 2);
            price.Property(money => money.Currency).HasColumnName("unit_price_currency").HasMaxLength(3);
        });

        builder.Ignore(line => line.LineTotal);
    }
}
