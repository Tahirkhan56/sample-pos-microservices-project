using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.Payments.Domain.Payments;

namespace Pos.Payments.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.Id).ValueGeneratedNever();

        builder.HasIndex(payment => payment.OrderId).IsUnique();
        builder.Property(payment => payment.OrderNumber).HasMaxLength(Payment.OrderNumberMaxLength).IsRequired();
        builder.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(payment => payment.ProviderPaymentId).HasMaxLength(Payment.ProviderPaymentIdMaxLength);
        builder.HasIndex(payment => payment.ProviderPaymentId);
        builder.Property(payment => payment.FailureReason).HasMaxLength(Payment.FailureReasonMaxLength);
        builder.Property(payment => payment.Version).IsConcurrencyToken();

        builder.ComplexProperty(payment => payment.Amount, amount =>
        {
            amount.Property(money => money.Amount).HasColumnName("amount").HasPrecision(18, 2);
            amount.Property(money => money.Currency).HasColumnName("currency").HasMaxLength(3);
        });

        builder.Ignore(payment => payment.DomainEvents);
    }
}
