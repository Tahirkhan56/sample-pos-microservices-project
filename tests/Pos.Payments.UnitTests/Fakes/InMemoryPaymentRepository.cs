using Pos.BuildingBlocks.Application.Abstractions;
using Pos.Payments.Domain.Payments;

namespace Pos.Payments.UnitTests.Fakes;

internal sealed class InMemoryPaymentRepository : IPaymentRepository, IUnitOfWork
{
    public List<Payment> Payments { get; } = [];

    public int SaveChangesCalls { get; private set; }

    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken) =>
        Task.FromResult(Payments.Find(payment => payment.OrderId == orderId));

    public Task<Payment?> GetByProviderPaymentIdAsync(string providerPaymentId, CancellationToken cancellationToken) =>
        Task.FromResult(Payments.Find(payment => payment.ProviderPaymentId == providerPaymentId));

    public void Add(Payment payment) => Payments.Add(payment);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(1);
    }
}
