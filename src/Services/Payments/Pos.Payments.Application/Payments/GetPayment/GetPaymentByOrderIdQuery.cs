using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Results;
using Pos.Payments.Domain.Payments;

namespace Pos.Payments.Application.Payments.GetPayment;

public sealed record GetPaymentByOrderIdQuery(Guid OrderId) : IQuery<PaymentResponse>;

internal sealed class GetPaymentByOrderIdQueryHandler(IPaymentRepository payments) : IQueryHandler<GetPaymentByOrderIdQuery, PaymentResponse>
{
    public async Task<Result<PaymentResponse>> HandleAsync(GetPaymentByOrderIdQuery query, CancellationToken cancellationToken)
    {
        Payment? payment = await payments.GetByOrderIdAsync(query.OrderId, cancellationToken);
        return payment is null ? PaymentErrors.NotFoundForOrder(query.OrderId) : payment.ToResponse();
    }
}
