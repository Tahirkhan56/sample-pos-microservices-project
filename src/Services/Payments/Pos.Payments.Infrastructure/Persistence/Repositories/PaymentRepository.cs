using Microsoft.EntityFrameworkCore;
using Pos.Payments.Domain.Payments;

namespace Pos.Payments.Infrastructure.Persistence.Repositories;

internal sealed class PaymentRepository(PaymentsDbContext dbContext) : IPaymentRepository
{
    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken) =>
        dbContext.Payments.FirstOrDefaultAsync(payment => payment.OrderId == orderId, cancellationToken);

    public Task<Payment?> GetByProviderPaymentIdAsync(string providerPaymentId, CancellationToken cancellationToken) =>
        dbContext.Payments.FirstOrDefaultAsync(payment => payment.ProviderPaymentId == providerPaymentId, cancellationToken);

    public void Add(Payment payment) => dbContext.Payments.Add(payment);
}
