namespace Pos.Payments.Domain.Payments;

public interface IPaymentRepository
{
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task<Payment?> GetByProviderPaymentIdAsync(string providerPaymentId, CancellationToken cancellationToken);

    void Add(Payment payment);
}
